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


//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorkerParametry : ContextBase
        {

            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }

            [Caption("Wartość A")]
            public double ValueA { get; set; }

            [Caption("Wartość B")]
            public double ValueB { get; set; }

            [Caption("Operacja liczbowa")]
            public MathOperation Operation { get; set; }


            public TemplateWorkerParametry(Context context) : base(context)
            {

                this.DataObliczen = Date.Today;
                this.ValueB = 0;
                this.ValueB = 0;
                this.Operation = MathOperation.Dodawanie;

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
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();
            //Pobieranie danych z Contextu
            Pracownik pracownik = null;
            Pracownik[] pracownicy = null;
            if (this.Cx.Contains(typeof(Pracownik[])))
            {
                pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])];
            }
            else
            {
                throw new InvalidOperationException("Nie znaleziono pracownika w kontekście.");
            }

            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {

                    // operacje liczbowe

                     for (int i = 0; i < pracownicy.Length; i++)
                     {
                         pracownik = pracownicy[i];
                         //Pobieramy obiekt z Nowo utworzonej sesji
                         var pracownikZSesja = nowaSesja.Get(pracownik);
                         double result = 0;
                         switch (this.Parametry.Operation)
                         {
                             case MathOperation.Dodawanie:
                                 result = this.Parametry.ValueA + this.Parametry.ValueB;
                                 break;
                             case MathOperation.Odejmowanie:
                                 result = this.Parametry.ValueA - this.Parametry.ValueB;
                                 break;
                             case MathOperation.Mnożenie:
                                 result = this.Parametry.ValueA * this.Parametry.ValueB;
                                 break;
                             case MathOperation.Dzielenie:
                                 if (this.Parametry.ValueB != 0)
                                    result = Math.Round((double)this.Parametry.ValueA / this.Parametry.ValueB, 2);
                                else
                                     throw new InvalidOperationException("Cannot divide by zero.");
                                 break;
                         }

                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = result;
                    }
                    

                    //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                    //Zatwierdzamy zmiany wykonane w sesji
                    trans.CommitUI();
                }
                //Zapisujemy zmiany
                nowaSesja.Save();
            }
        }

    }
}
