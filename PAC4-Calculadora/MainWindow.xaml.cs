using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PAC4_Calculadora
{
    public partial class MainWindow : Window
    {
        // VARIABLES D'ESTAT DE LA CALCULADORA
        
        /// <summary>
        /// Llista dels números i operadors de l'operació que s'està fent --> elements
        /// </summary>
        private List<string> _tokens = new List<string>();

        /// <summary>
        /// Número que l'usuari està teclejant en aquest moment.
        /// Es mostra coma per pantalla per es guarda amb punt.
        /// </summary>
        private string _entradaActual = "0";

        /// <summary>
        /// Boleà que indica si el pròxim dígit ha d'iniciar número nou.
        /// Sobreescriu a pantalla o afegeix al costat de l'actual.
        /// </summary>
        private bool _nouNumero = true;

        /// <summary>
        /// Màxim de dígits permesos per entrada.
        /// </summary>
        private const int MAX_DIGITS_ENTRADA = 16;

        /// <summary>
        /// Constructor de la classe MainWindow. Inicialitza els components visuals de la calculadora.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Mètode per actualitzar la pantalla amb el text formatat correctament (coma en lloc de punt).
        /// </summary>
        /// <param name="text">El text que es vol mostrar a la pantalla.</param>
        private void ActualitzarPantalla(string text)
        {
            Pantalla.Text = text;
        }

        /// <summary>
        /// Mètode per mostrar el resultat final a pantalla, aplicant format correcte (notació científica si cal).
        /// </summary>
        /// <param name="valor">El valor numèric del resultat que es vol mostrar.</param>
        private void MostrarResultat(double valor)
        {
            Pantalla.Text = FormatarResultat(valor);
        }

        /// <summary>
        /// Mètode per actualitzar la línia d'expressió que mostra operació acumulada. Converteix punts a comes per a visualització.
        /// </summary>
        private void ActualitzarExpressio()
        {
            if (_tokens.Count == 0)
            {
                PantallaExpressio.Text = "";
                return;
            }

            // Construïm el text unint els tokens separats per espai.
            // Convertim el punt intern a coma per a la visualització dels números.
            string expressio = string.Join(" ", _tokens.Select(t =>
                esOperador(t) ? t : t.Replace('.', ',')));

            PantallaExpressio.Text = expressio;
        }

        /// <summary>
        /// Mètode per formatar el resultat numèric.
        /// </summary>
        /// <param name="valor">El valor numèric a formatar.</param>
        /// <returns>La cadena de text amb el número formatat correctament (amb coma decimal o notació científica).</returns>
        private string FormatarResultat(double valor)
        {
            // Si el resultat és infinit o NaN (Not a Number), mostrem "Error".
            if (double.IsInfinity(valor) || double.IsNaN(valor))
                return "Error";

            // Si el valor és molt gran o molt petit, usem notació científica.
            double abs = Math.Abs(valor);
            if (valor != 0 && (abs >= 1e15 || abs < 1e-9))
            {
                // "e10" → notació científica amb 10 dígits decimals.
                string cientific = valor.ToString("e10", CultureInfo.InvariantCulture);

                // Separem part decimal i exponent per eliminar zeros innecessaris.
                string[] parts = cientific.Split('e');
                string mantissa = parts[0].TrimEnd('0').TrimEnd('.');
                string exponent = parts[1];

                // Convertim l'exponent a int per eliminar zeros innecessaris i afegir el signe "+" si és positiu.
                int expVal = int.Parse(exponent);
                string expStr = expVal >= 0 ? "e+" + expVal : "e" + expVal;

                return (mantissa + expStr).Replace('.', ',');
            }

            // Converteix el número a text amb un màxim de 15 dígits i punt decimal.
            string resultat = valor.ToString("G15", CultureInfo.InvariantCulture);

            // Convertim el punt a coma per a la visualització
            return resultat.Replace('.', ',');
        }


        /// <summary>
        /// Gestiò dels botons numèrics (0-9, "00" i "·" per a la coma decimal)
        /// </summary>
        /// <param name="sender">L'objecte que ha disparat l'esdeveniment (el botó premut).</param>
        /// <param name="e">Els arguments de l'esdeveniment.</param>
        private void BotoDigit_Click(object sender, RoutedEventArgs e)
        {
            // Si la pantalla mostra "Error", qualsevol entrada numèrica reinicia la calculadora.
            if (Pantalla.Text == "Error")
            {
                BotoAC_Click(sender, e);
                return;
            }

            // Si venim d'un resultat (la pantalla d'expressió conté "="), la buidem abans de començar un nou número
            if (PantallaExpressio.Text.Contains("="))
            {
                PantallaExpressio.Text = "";
            }

            Button boto = (Button)sender;
            string valor = boto.Content.ToString();

            // Convertim "·" a "." per al processament intern, però mantenim la coma a la pantalla.
            if (valor == "·") valor = ".";

            // Si estem començant un número nou, reiniciem entrada actual.
            if (_nouNumero)
            {
                _entradaActual = "";
                _nouNumero = false;
            }

            // Comptem els dígits actuals (sense comptar el punt ni el signe negatiu).
            int digitsSig = _entradaActual.Replace(".", "").Replace("-", "").Length;

            // Si l'entrada no és punt ni "00", ja tenim el màxim de dígits, ignorem entrada.
            if (valor != "." && valor != "00" && digitsSig >= MAX_DIGITS_ENTRADA)
                return;

            // Validació per al punt decimal: només es permet un i no pot ser el primer caràcter (excepte si és "0").
            if (valor == ".")
            {
                // Si ja hi ha un punt, no en permetem un altre.
                if (_entradaActual.Contains(".")) return;

                // Si l'entrada està buida o només té un signe negatiu, afegim un "0" abans del punt per evitar formats com "." o "-.".
                if (_entradaActual == "" || _entradaActual == "-") _entradaActual += "0";
            }

            // Validació per al "00": només es permet si no supera el límit de dígits i no afegeix zeros innecessaris.
            if (valor == "00")
            {
                // Si l'entrada està buida o és només "0", no afegim "00".
                if (_entradaActual == "" || _entradaActual == "0")
                {
                    ActualitzarPantalla("0");
                    return;
                }

                // Si afegim "00" superem el límit de dígits, no ho permetem.
                if (digitsSig + 2 > MAX_DIGITS_ENTRADA) return;
            }

            // Afegim dígit a entrada actual. Si entrada actual és "0" i el valor no és ".", el substituïm pel nou dígit per evitar formats com "05".
            if (_entradaActual == "0" && valor != ".")
                _entradaActual = valor;
            else
                _entradaActual += valor;

            // Actualitzem la pantalla amb la nova entrada, convertint el punt a coma per a la visualització.
            ActualitzarPantalla(_entradaActual.Replace('.', ','));
        }


        /// <summary>
        /// Gestiò dels botons d'operador (+, -, ×, ÷)
        /// </summary>
        /// <param name="sender">L'objecte que ha disparat l'esdeveniment (el botó premut).</param>
        /// <param name="e">Els arguments de l'esdeveniment.</param>
        private void BotoOperador_Click(object sender, RoutedEventArgs e)
        {
            if (Pantalla.Text == "Error") return;

            Button boto = (Button)sender;
            string op = boto.Content.ToString();

            if (_entradaActual != "")
            {
                // Si hi ha una entrada actual, l'afegim a la llista de tokens abans de l'operador.
                _tokens.Add(_entradaActual);
            }
            else if (_tokens.Count > 0 && esOperador(_tokens.Last()))
            {
                // Si no hi ha entrada actual però l'últim element és un operador, substituïm operador per permetre canviar element ("5 + " --> "5 - ").
                _tokens[_tokens.Count - 1] = op;
                return;
            }
            else if (_tokens.Count == 0)
            {
                // Si no hi ha elements i no hi ha entrada, assumim que l'usuari vol començar amb "0" ("+ 5" --> "0 + 5").
                _tokens.Add("0");
            }

            _tokens.Add(op);
            _entradaActual = "";
            _nouNumero = true;

            // Actualitzem la línia d'expressió perquè mostri l'operació acumulada
            ActualitzarExpressio();
        }


        /// <summary>
        /// Botó C — Clear
        /// </summary>
        /// <param name="sender">L'objecte que ha disparat l'esdeveniment (el botó premut).</param>
        /// <param name="e">Els arguments de l'esdeveniment.</param>
        private void BotoC_Click(object sender, RoutedEventArgs e)
        {
            if (Pantalla.Text == "Error")
            {
                BotoAC_Click(sender, e);
                return;
            }

            _entradaActual = "0";
            _nouNumero = true;
            ActualitzarPantalla("0");
            // C esborra l'entrada però conserva l'expressió acumulada als tokens
            ActualitzarExpressio();
        }


        /// <summary>
        /// Botó AC — All Clear
        /// </summary>
        /// <param name="sender">L'objecte que ha disparat l'esdeveniment (el botó premut).</param>
        /// <param name="e">Els arguments de l'esdeveniment.</param>
        private void BotoAC_Click(object sender, RoutedEventArgs e)
        {
            _tokens.Clear();
            _entradaActual = "0";
            _nouNumero = true;
            ActualitzarPantalla("0");
            // AC buida també la línia d'expressió
            ActualitzarExpressio();
        }


        /// <summary>
        /// Botó "=" — Avalua expressió actual i mostra resultat.
        /// </summary>
        /// <param name="sender">L'objecte que ha disparat l'esdeveniment (el botó premut).</param>
        /// <param name="e">Els arguments de l'esdeveniment.</param>
        private void BotoIgual_Click(object sender, RoutedEventArgs e)
        {
            if (Pantalla.Text == "Error") return;

            // Afegim l'últim número pendent a la llista
            if (_entradaActual != "")
                _tokens.Add(_entradaActual);

            if (_tokens.Count == 0) return;

            // Si l'últim element és un operador (ex: "5 + ="), és un error de sintaxi
            if (esOperador(_tokens.Last()))
            {
                MostrarError();
                return;
            }

            // Mostrem l'expressió COMPLETA a la línia d'historial (incloent l'operand final i el "=")
            // Així l'usuari veu exactament què s'ha calculat, o per què ha fallat si hi ha error.
            // Exemple: "5 ÷ 0 ="  o  "1 + 3 × 5 ="
            string expressioCompleta = string.Join(" ", _tokens.Select(t =>
                esOperador(t) ? t : t.Replace('.', ',')));
            PantallaExpressio.Text = expressioCompleta + " =";

            try
            {
                double resultat = Calcula(_tokens);

                // Mostrem el resultat a pantalla amb format correcte.
                MostrarResultat(resultat);

                // Reiniciem l'estat intern per a una nova operació, però mantenim el resultat
                // com a entrada actual per permetre operacions encadenades.
                _entradaActual = resultat.ToString(CultureInfo.InvariantCulture);
                _tokens.Clear();
                _nouNumero = true;
                // Mantenim l'expressió completa a sobre de la línia del resultat
            }
            catch (DivideByZeroException)
            {
                // En cas d'error, PantallaExpressio JA mostra "5 ÷ 0 ="
                // No la toquem: l'usuari pot veure per què ha fallat.
                MostrarError();
            }
            catch (Exception)
            {
                MostrarError();
            }
        }


        // Mètodes auxiliars

        /// <summary>
        /// Mètode per mostrar un error a pantalla i reiniciar l'estat de la calculadora.
        /// No esborra la línia d'expressió perquè l'usuari pugui veure què ha intentat calcular i per què ha fallat.
        /// </summary>
        private void MostrarError()
        {
            _tokens.Clear();
            _entradaActual = "0";
            _nouNumero = true;
            Pantalla.Text = "Error";
            // PantallaExpressio es deixa intacta intencionadament
        }

        /// <summary>
        /// Mètode per determinar si un element és un operador vàlid.
        /// </summary>
        /// <param name="token">L'element (cadena de text) a comprovar.</param>
        /// <returns>Retorna cert si és un operador vàlid (+, -, ×, *, ÷, /, x, X), fals en cas contrari.</returns>
        private bool esOperador(string token)
        {
            string t = token.Trim();
            return t == "+" || t == "-" || t == "×" || t == "*" || t == "÷" || t == "/" || t == "x" || t == "X";
        }

        /// <summary>
        /// Mètode per calcular el resultat d'una expressió representada com a llista d'elements' (números i operadors).
        /// </summary>
        /// <param name="expressio">La llista d'elements (tokens) que formen l'expressió matemàtica a avaluar.</param>
        /// <returns>El resultat numèric de l'operació matemàtica resolta.</returns>
        private double Calcula(List<string> expressio)
        {
            List<string> pass1 = new List<string>();

            // Pas 1: Multiplicacions i Divisions (prioritat alta)
            for (int i = 0; i < expressio.Count; i++)
            {
                // Agafem element actual i fem trim() per eliminar espais innecessaris.
                string token = expressio[i].Trim();

                // Si l'element és un operador de multiplicació o divisió, el processem amb operand anterior (últim element de pass1) i el següent (expressio[i + 1]).
                if (token == "×" || token == "*" || token == "x" || token == "X" ||
                    token == "÷" || token == "/")
                {
                    // Validació: si no hi ha operand anterior o següent, és un error de sintaxi.
                    double num1 = double.Parse(pass1.Last(), CultureInfo.InvariantCulture);
                    double num2 = double.Parse(expressio[i + 1].Trim(), CultureInfo.InvariantCulture);
                    double parcial;

                    if (token == "×" || token == "*" || token == "x" || token == "X")
                        parcial = num1 * num2;
                    else
                    {
                        if (num2 == 0) throw new DivideByZeroException();
                        parcial = num1 / num2;
                    }

                    // Substituïm últim número a pass1 pel resultat parcial i saltem el següent número perquè ja l'hem consumit.
                    pass1[pass1.Count - 1] = parcial.ToString(CultureInfo.InvariantCulture);
                    i++; // Saltem el següent número perquè ja l'hem processat
                }
                else
                {
                    pass1.Add(token);
                }
            }

            // Pas 2: Sumes i Restes (prioritat baixa)
            double resultat = double.Parse(pass1[0], CultureInfo.InvariantCulture);

            for (int i = 1; i < pass1.Count; i += 2)
            {
                string op = pass1[i].Trim();
                double num2 = double.Parse(pass1[i + 1], CultureInfo.InvariantCulture);

                if (op == "+") resultat += num2;
                else if (op == "-") resultat -= num2;
            }

            return resultat;
        }
    }
}