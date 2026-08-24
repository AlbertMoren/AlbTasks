using System;

namespace AlbTasks.Models
{
    public class AtividadeDiaria
    {
        public string Texto { get; set; } = string.Empty;
        public DateTime HorarioAlerta { get; set; }
        public bool Concluida { get; set; } = false;
    }
}