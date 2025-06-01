using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.KadryPlace;
using Soneta.Types;
using Rekrutacja.Workers.Template;
using Soneta.Handel;
using static Rekrutacja.Workers.Template.TemplateWorker.TemplateWorkerParametry;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorkerParametry : ContextBase
        {
            public enum Figura
            {
                Kwadrat,
                Prostokąt,
                Trojkąt,
                Koło
            }

            [Caption("A")]
            public double A { get; set; }

            [Caption("B")]
            public double B { get; set; }

            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }

            [Caption("Figura")]
            public Figura figura{ get; set; }

            public TemplateWorkerParametry(Context context) : base(context)
            {
                this.DataObliczen = Date.Today;
            }
        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę
        [Context]
        public TemplateWorkerParametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator",
           Description = "Prosty kalkulator ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public void WykonajAkcje()
        {
            Waliduj();
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            var wynik = WykonajObliczenia();
            DebuggerSession.MarkLineAsBreakPoint();

            //Pobieranie danych z Contextu
            Pracownik[] pracownicy = null;
            if (this.Cx.Contains(typeof(Pracownik[])))
            {
                pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])];
            }

            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {
                    foreach (var pracownik in pracownicy)
                    {
                        var pracownikZSesja = nowaSesja.Get(pracownik);
                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = wynik;
                    }

                    trans.CommitUI();
                }
                //Zapisujemy zmiany
                nowaSesja.Save();
            }
        }

        private double WykonajObliczenia()
        {
            double wynik;

            switch (this.Parametry.figura)
            {
                case Figura.Kwadrat:
                    wynik = this.Parametry.A * this.Parametry.A;
                    break;

                case Figura.Prostokąt:
                    wynik = this.Parametry.A * this.Parametry.B;
                    break;

                case Figura.Trojkąt:
                    wynik = (this.Parametry.A * this.Parametry.B) / 2;
                    break;

                case Figura.Koło:
                    wynik = Math.PI * this.Parametry.A * this.Parametry.A;
                    break;

                default:
                    throw new Exception("Nieznana figura.");
            }

            return (int)Math.Round(wynik, MidpointRounding.AwayFromZero);
        }

        private void Waliduj()
        {
            if (this.Parametry.A <= 0)
                throw new Exception("Wartość A musi być dodatnia.");

            if ((this.Parametry.figura == Figura.Prostokąt || this.Parametry.figura == Figura.Trojkąt)
                && this.Parametry.B <= 0)
            {
                throw new Exception("Wartość B musi być dodatnia.");
            }
        }
    }
}