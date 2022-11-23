using System;
using System.Collections.Generic;
using SimSharp;

namespace Biblioteca
{
    public class Library
    {
        Simulation env;
        Random random;
        Resource lookers;
        Resource stands;
        Resource readingArea;
        Resource librarian;

        int SEED = 666;
        int TOTAL_CLIENTS = 50;
        int CAPACITY_LOOKER = 5;
        int CAPACITY_STAND = 4;
        int CAPACITY_READING_AREA = 6;
        int NUM_LIBRARIANS = 1;
        int TOTAL_CLIENTS_IN_READING_AREA = 0;
        int TOTAL_CLIENTS_IN_RENT_BOOK = 0;
        int TOTAL_CLIENTS_IN_GO_TO_LOOKER = 0;
        int TOTAL_CLIENTS_IN_STANT = 0;

        double TIME_ARRIVE_CLIENT = 10;

        double TOTAL_TIME_AWAITED_SELECT = 0;
        double TOTAL_TIME_AWAITED_READ = 0;
        double TOTAL_TIME_AWAITED_RENT = 0;
        double TOTAL_TIME_AWAITED_GO_TO_LOOKER = 0;
        double TIME_SERVICE = 0;
        double TIME_END = 0;

        public Library()
        {
            this.env = new Simulation();
            this.random = new Random(this.SEED);
            this.lookers = new Resource(this.env, this.CAPACITY_LOOKER);
            this.stands = new Resource(this.env, this.CAPACITY_STAND);
            this.readingArea = new Resource(this.env, this.CAPACITY_READING_AREA);
            this.librarian = new Resource(this.env, this.NUM_LIBRARIANS);
        }

        public IEnumerable<Event> GoToLooker(string name)
        {
            double arrive = this.env.NowD;
            int timeGoToLooker = 15;
            double await = this.random.Next(5, timeGoToLooker + 1);

            yield return this.env.TimeoutD(await);

            double finishGoToLooker = this.env.NowD;
            double awaited = finishGoToLooker - arrive;
            this.TOTAL_TIME_AWAITED_GO_TO_LOOKER += awaited;
            this.TIME_SERVICE += finishGoToLooker;

            this.env.Log(string.Format("---> {0} terminó de buscar un libro en el minuto {1:0.00} habiendo esperado {2:0.00}", name, finishGoToLooker, awaited));
        }

        public IEnumerable<Event> SelectBook(string name)
        {
            double arrive = this.env.NowD;
            int timeToSelectBook = 15;
            double await = this.random.Next(5, timeToSelectBook + 1);

            yield return this.env.TimeoutD(await);

            double finishSelectBook = this.env.NowD;
            double awaited = finishSelectBook - arrive;
            this.TOTAL_TIME_AWAITED_SELECT += awaited;
            this.TIME_SERVICE += finishSelectBook;

            this.env.Log(string.Format("---> {0} terminó de buscar un libro en el minuto {1:0.00} habiendo esperado {2:0.00}", name, finishSelectBook, awaited));
        }
        
        public IEnumerable<Event> ReadBook(string name)
        {
            double arrive = this.env.NowD;
            int timeToReadBook = 60;
            double await = this.random.Next(25, timeToReadBook + 1);

            yield return this.env.TimeoutD(await);

            double finishReadBook = this.env.NowD;
            double awaited = finishReadBook - arrive;
            this.TOTAL_TIME_AWAITED_READ += awaited;
            this.TIME_SERVICE += finishReadBook;

            this.env.Log(string.Format("---> {0} terminó de leer el libro en el minuto {1:0.00} habiendo esperado {2:0.00}", name, finishReadBook, awaited));
        }
        public IEnumerable<Event> RentBook(string name)
        {
            double arrive = this.env.NowD;
            int timeToRentBook = 10;
            double await = this.random.Next(5, timeToRentBook + 1);

            yield return this.env.TimeoutD(await);

            double finishRentBook = this.env.NowD;
            double awaited = finishRentBook - arrive;
            this.TOTAL_TIME_AWAITED_RENT += awaited;
            this.TIME_SERVICE += finishRentBook;

            this.env.Log(string.Format("---> {0} terminó de solicitar el libro en el minuto {1:0.00} habiendo esperado {2:0.00}", name, finishRentBook, awaited));
        }

        public IEnumerable<Event> User(string name)
        {
            // Tiempo de llegada
            double arrive = this.env.NowD;
            this.env.Log(string.Format("El usuario {0} llego a la biblioteca en el minuto {1}", name, arrive));

            double probabilityLeft = this.random.Next(0, 100 + 1);

            double probabilityGoToLooker = 40;

            if (probabilityLeft <= probabilityGoToLooker)
            {
                this.TOTAL_CLIENTS_IN_GO_TO_LOOKER++;
                using (Request looker = this.lookers.Request())
                {
                    yield return looker;

                    double goToLooker = this.env.NowD;

                    this.env.Log(string.Format("---> {0} pasa al looker en el minuto {1:0.00}", name, goToLooker));

                    yield return this.env.Process(this.GoToLooker(name));
                }
            }

            probabilityLeft = this.random.Next(0, 100 + 1);

            this.TOTAL_CLIENTS_IN_STANT++;
            using (Request stand = this.stands.Request()) {
                // Select Book
                yield return stand;

                double goToStand = this.env.NowD;

                this.env.Log(string.Format("---> {0} pasa al estante en el minuto {1:0.00}", name, goToStand));

                yield return this.env.Process(this.SelectBook(name));
            }

            double probabilityReadBook = 20;
            if (probabilityLeft <= probabilityReadBook)
            {
                this.TOTAL_CLIENTS_IN_READING_AREA++;
                using (Request areaR = this.readingArea.Request())
                {
                    // Leyendo xd
                    yield return areaR;

                    double goToReadingArea = this.env.NowD;

                    this.env.Log(string.Format("---> {0} pasa al area de lectura en el minuto {1:0.00}", name, goToReadingArea));

                    yield return this.env.Process(this.ReadBook(name));
                }

                probabilityLeft = this.random.Next(0, 100 + 1);
                double probabilityRentBook = 60;

                if (probabilityLeft <= probabilityRentBook)
                {
                    this.TOTAL_CLIENTS_IN_RENT_BOOK++;
                    using (Request libr = this.librarian.Request())
                    {
                        // Renta libro
                        yield return libr;

                        double goToRentBook = this.env.NowD;

                        this.env.Log(string.Format("---> {0} pasa con el bibliotecario para rentar el libro en el minuto {1:0.00}", name, goToRentBook));

                        yield return this.env.Process(this.RentBook(name));
                    }
                }
            }

            double goOut = this.env.NowD;
            this.env.Log(string.Format("El usuario {0} salió de la biblioteca en el minuto {1}", name, goOut));
            this.TIME_END = goOut;
        }

        public IEnumerable<Event> Principal()
        {
            for (int i = 0; i < this.TOTAL_CLIENTS; i++)
            {
                double R = this.random.NextDouble();
                double arrive = - this.TIME_ARRIVE_CLIENT * Math.Log(R);
                yield return this.env.TimeoutD(arrive);
                this.env.Process(this.User("Juanito " + i));
            }
        }

        public void RunSimulation()
        {
            this.env.Log("----- Bienvenido a la Biblioteca  -----\n");
            this.env.Process(this.Principal());
            this.env.Run();

            this.env.Log("\n---------------------------------------\n");
            this.env.Log("Indicadores Obtenidos");

            double avgTimes = this.TOTAL_TIME_AWAITED_GO_TO_LOOKER / this.TOTAL_CLIENTS_IN_GO_TO_LOOKER;
            this.env.Log(string.Format("Tiempo de espera promedio en el uso de los lookers {0:0.00}", avgTimes));
            
            avgTimes = this.TOTAL_TIME_AWAITED_SELECT / this.TOTAL_CLIENTS_IN_STANT;
            this.env.Log(string.Format("Tiempo de espera promedio en la seleccion de libros {0:0.00}", avgTimes));
            
            avgTimes = this.TOTAL_TIME_AWAITED_READ / this.TOTAL_CLIENTS_IN_READING_AREA;
            this.env.Log(string.Format("Tiempo de espera promedio en la lectura de libros {0:0.00}", avgTimes));
            
            avgTimes = this.TOTAL_TIME_AWAITED_RENT / this.TOTAL_CLIENTS_IN_RENT_BOOK;
            this.env.Log(string.Format("Tiempo de espera promedio en la renta de libros {0:0.00}", avgTimes));

            double avarageUse = 100 * (this.TIME_SERVICE / this.TIME_END) / this.TOTAL_CLIENTS;
            this.env.Log(string.Format("Uso promedio de la instalación {0:0.00}%", avarageUse));
        }
    }
}
