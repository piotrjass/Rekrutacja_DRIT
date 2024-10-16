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
             public string ValueA { get; set; }

             [Caption("Wartość B")]
             public string ValueB { get; set; }

             [Caption("Figura")]
             public Figure FigureOperation { get; set; }

            public TemplateWorkerParametry(Context context) : base(context)
            {

                this.DataObliczen = Date.Today;
                this.ValueB = "";
                this.ValueB = "";
                this.FigureOperation = Figure.Kwadrat;
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

                    for (int i = 0; i < pracownicy.Length; i++)
                    {
                        pracownik = pracownicy[i];
                        var pracownikZSesja = nowaSesja.Get(pracownik);
                        int result = 0;
                        int parameterA = StringToIntConverter.Convert(this.Parametry.ValueA);
                        int parameterB = StringToIntConverter.Convert(this.Parametry.ValueB);

                     switch(this.Parametry.FigureOperation)
                          {
                              case Figure.Kwadrat:
                                  result = (int)Math.Pow(parameterA, 2);
                                  break;
                              case Figure.Prostokat:
                                  result = parameterA * parameterB;
                                  break;
                              case Figure.Trojkat:
                                  result = (int)((parameterA * parameterB) / 2.0);
                                  break;
                              case Figure.Kolo:
                                  result = (int)(Math.PI * Math.Pow(parameterA, 2)); 
                                  break;
                              default:
                                  throw new InvalidOperationException("Unknown figure.");
                          }

                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = (double)result;
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
