using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AlbTasks.Models;

namespace AlbTasks.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly GerenciadorArquivos _arquivos;
        private readonly Dictionary<string, List<string>> _templates;

        public ObservableCollection<DemandaAtiva> DemandasAtivas { get; set; }

        private DemandaAtiva? _demandaSelecionada;
        public DemandaAtiva? DemandaSelecionada
        {
            get => _demandaSelecionada;
            set
            {
                _demandaSelecionada = value;
                OnPropertyChanged();
                AtualizarTextoPassoAtual();
            }
        }

        private string _textoPassoAtual = "Nenhuma demanda selecionada";
        public string TextoPassoAtual
        {
            get => _textoPassoAtual;
            set
            {
                _textoPassoAtual = value;
                OnPropertyChanged(); 
            }
        }

        public ICommand AvancarPassoCommand { get; }

        public MainViewModel()
        {
            _arquivos = new GerenciadorArquivos();
            _templates = _arquivos.CarregarTemplates();
            
            var estadoSalvo = _arquivos.CarregarEstado();
            DemandasAtivas = new ObservableCollection<DemandaAtiva>(estadoSalvo);

            AvancarPassoCommand = new RelayCommand(AvancarPasso);
        }

        private void AvancarPasso()
        {
            if (DemandaSelecionada == null || !_templates.ContainsKey(DemandaSelecionada.TipoPipeline)) 
                return;

            var passosDaPipeline = _templates[DemandaSelecionada.TipoPipeline];
            DemandaSelecionada.IndicePassoAtual++;

            if (DemandaSelecionada.IndicePassoAtual >= passosDaPipeline.Count)
            {
                DemandasAtivas.Remove(DemandaSelecionada);
                TextoPassoAtual = "Demanda Finalizada!";
            }
            else
            {
                AtualizarTextoPassoAtual();
            }

            _arquivos.SalvarEstado(DemandasAtivas.ToList());
        }

        private void AtualizarTextoPassoAtual()
        {
            if (DemandaSelecionada == null) return;

            var passos = _templates[DemandaSelecionada.TipoPipeline];
            if (DemandaSelecionada.IndicePassoAtual < passos.Count)
            {
                TextoPassoAtual = passos[DemandaSelecionada.IndicePassoAtual];
            }
        }
    }
}