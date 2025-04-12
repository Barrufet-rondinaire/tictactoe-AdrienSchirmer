using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        public static HttpClient bdtictactoe;
        public static List<Participant> ParticipantsValids = new();
        public static List<Participant> ParticipantsInvalids = new();

        static async Task Main(string[] args)
        {
            string url = "http://localhost:8080/";
            bdtictactoe = new HttpClient()
            {
                BaseAddress = new Uri(url)
            };

            await GetParticipants();
            await RevisarPartides();
            GuanyadorPerVictories();
        }

        private static void GuanyadorPerVictories()
        {
            List<string> Guanyadors = new();
            int victoriesguanyador = 0;

            foreach (var Participant in ParticipantsValids)
            {
                if (Participant.Victories > victoriesguanyador)
                {
                    victoriesguanyador = Participant.Victories;
                    Guanyadors.Clear();
                    Guanyadors.Add(Participant.Nom);
                }

                else if (Participant.Victories == victoriesguanyador)
                {
                    Guanyadors.Add(Participant.Nom);
                }
            }
            
            if (Guanyadors.Count == 1)
            {
                Console.WriteLine($"Guanyador/a: {Guanyadors[0]} amb {victoriesguanyador} victories!");
            }
            else
            {
                Console.WriteLine("Empat entre els següents jugadors:");
                foreach (var guanyador in Guanyadors)
                {
                    Console.WriteLine(guanyador + " amb " + victoriesguanyador + " victories");
                }
            }
        }


        private static async Task RevisarPartides()
        {
            for (int i = 1; i < 10000; i++)
            {
                var partida = await bdtictactoe.GetFromJsonAsync<Partida>($"/partida/{i}");

                if (partida == null)
                {
                    continue;
                }

                bool invalida = false;
                foreach (var participant in ParticipantsInvalids)
                {
                    if (partida.jugador1 == participant.Nom || partida.jugador2 == participant.Nom)
                    {
                        invalida = true;
                        break;
                    }
                }

                if (!invalida)
                {
                    CalcularGuanyador(partida);
                }
            }
        }


        private static void CalcularGuanyador(Partida partida)
        {
            var tauler = partida.Tauler;
            string guanyador = null;

            for (int i = 0; i < 3; i++)
            {
                if (tauler[i][0] == tauler[i][1] && tauler[i][1] == tauler[i][2] && tauler[i][0] != '.')
                {
                    if (tauler[i][0] == 'X') guanyador = partida.jugador1;
                    else guanyador = partida.jugador2;
                    break;
                }
            }

            if (guanyador == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (tauler[0][i] == tauler[1][i] && tauler[1][i] == tauler[2][i] && tauler[0][i] != '.')
                    {
                        if (tauler[0][i] == 'X') guanyador = partida.jugador1;
                        else guanyador = partida.jugador2;
                        break;
                    }
                }
            }

            if (guanyador == null)
            {
                if (tauler[0][0] == tauler[1][1] && tauler[1][1] == tauler[2][2] && tauler[0][0] != '.')
                {
                    if (tauler[0][0] == 'X') guanyador = partida.jugador1;
                    else guanyador = partida.jugador2;
                }
                else if (tauler[0][2] == tauler[1][1] && tauler[1][1] == tauler[2][0] && tauler[0][2] != '.')
                {
                    if (tauler[0][2] == 'X') guanyador = partida.jugador1;
                    else guanyador = partida.jugador2;
                }
            }

            if (guanyador != null)
            {
                foreach (var participant in ParticipantsValids)
                {
                    if (participant.Nom == guanyador)
                    {
                        participant.Victories++;
                        break;
                    }
                }
            }
        }

        
        private static async Task GetParticipants()
        {
            string pattern = @"participant (?<nom>[A-Za-z]+ [A-Za-z'-]+).*representa(nt)? (a|de) (?<pais>[A-Za-z]+)";
            string jsonText = await bdtictactoe.GetStringAsync("/jugadors");
            List<string> linies = JsonSerializer.Deserialize<List<string>>(jsonText);

            foreach (var linia in linies)
            {
                Match match = Regex.Match(linia, pattern);
                if (match.Success)
                {
                    var participant = new Participant(match.Groups["nom"].Value, match.Groups["pais"].Value);
                    if (linia.Contains("desqualificada") || linia.Contains("desqualificat"))
                        ParticipantsInvalids.Add(participant);
                    else
                        ParticipantsValids.Add(participant);
                }
            }
        }
    }
}
