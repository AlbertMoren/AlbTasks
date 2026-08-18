namespace AlbTasks.Models
{
    public class DemandaAtiva
    {
        public string NomeDemanda { get; set; } = string.Empty;
        public string TipoPipeline { get; set; } = string.Empty;
        public int IndicePassoAtual { get; set; }
    }
}