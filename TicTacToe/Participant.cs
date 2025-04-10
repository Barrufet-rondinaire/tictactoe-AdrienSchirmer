namespace TicTacToe;

class Participant
{
    public string Nom { get; set; }
    public string Pais { get; set; }
        
    public int Victories { get; set; }

    public Participant(string nom, string pais)
    {
        Nom = nom;
        Pais = pais;
    }
}