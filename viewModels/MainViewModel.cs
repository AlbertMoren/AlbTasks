using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlbTasks.Models;

namespace AlbTasks.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly GerenciadorArquivos _arquivos;
        private readonly Dictionary<string, List<string>> _templates;

        public ObservableCollection<DemandaAtiva> DemandasAtivas { get; set; }
        public ObservableCollection<string> TiposDePipeline { get; set; }

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

        private string _textoPassoAtual = "Tudo limpo! Nenhuma demanda pendente. ☕";
        public string TextoPassoAtual
        {
            get => _textoPassoAtual;
            set { _textoPassoAtual = value; OnPropertyChanged(); }
        }

        // --- PROPRIEDADES: NOVA DEMANDA ---
        private bool _isFormularioVisivel;
        public bool IsFormularioVisivel
        {
            get => _isFormularioVisivel;
            set { _isFormularioVisivel = value; OnPropertyChanged(); }
        }

        private string _novaDemandaNome = string.Empty;
        public string NovaDemandaNome
        {
            get => _novaDemandaNome;
            set { _novaDemandaNome = value; OnPropertyChanged(); }
        }

        private string? _novaDemandaPipeline; // Null ajuda o texto cinza a aparecer
        public string? NovaDemandaPipeline
        {
            get => _novaDemandaPipeline;
            set { _novaDemandaPipeline = value; OnPropertyChanged(); }
        }

        // --- PROPRIEDADES: GERENCIAR PIPELINE ---
        private bool _isFormularioPipelineVisivel;
        public bool IsFormularioPipelineVisivel
        {
            get => _isFormularioPipelineVisivel;
            set { _isFormularioPipelineVisivel = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> OpcoesEdicaoPipeline { get; set; } = new ObservableCollection<string>();

        private string _opcaoEdicaoSelecionada = string.Empty;
        public string OpcaoEdicaoSelecionada
        {
            get => _opcaoEdicaoSelecionada;
            set
            {
                _opcaoEdicaoSelecionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModoEdicao));
                CarregarDadosPipelineParaEdicao();
            }
        }

        public bool IsModoEdicao => OpcaoEdicaoSelecionada != "➕ Criar Nova Pipeline" && !string.IsNullOrEmpty(OpcaoEdicaoSelecionada);
        private string _pipelineOriginalNome = string.Empty;

        private string _novoPipelineNome = string.Empty;
        public string NovoPipelineNome
        {
            get => _novoPipelineNome;
            set { _novoPipelineNome = value; OnPropertyChanged(); }
        }

        private string _novoPipelinePassos = string.Empty;
        public string NovoPipelinePassos
        {
            get => _novoPipelinePassos;
            set { _novoPipelinePassos = value; OnPropertyChanged(); }
        }

        // --- COMANDOS ---
        public ICommand AvancarPassoCommand { get; }
        public ICommand AbrirFormCommand { get; }
        public ICommand CancelarFormCommand { get; }
        public ICommand SalvarNovaDemandaCommand { get; }
        
        public ICommand AbrirFormPipelineCommand { get; }
        public ICommand CancelarFormPipelineCommand { get; }
        public ICommand SalvarNovoPipelineCommand { get; }
        public ICommand ExcluirPipelineCommand { get; }

        public MainViewModel()
        {
            _arquivos = new GerenciadorArquivos();
            _templates = _arquivos.CarregarTemplates();
            
            var estadoSalvo = _arquivos.CarregarEstado();
            DemandasAtivas = new ObservableCollection<DemandaAtiva>(estadoSalvo);
            TiposDePipeline = new ObservableCollection<string>(_templates.Keys);

            // Inicia já puxando a primeira demanda para a tela
            DemandaSelecionada = DemandasAtivas.FirstOrDefault();
            AtualizarTextoPassoAtual();

            AvancarPassoCommand = new RelayCommand(AvancarPasso);
            
            AbrirFormCommand = new RelayCommand(() => 
            {
                NovaDemandaNome = string.Empty;
                NovaDemandaPipeline = null; 
                IsFormularioVisivel = true;
            });
            
            CancelarFormCommand = new RelayCommand(() => 
            {
                IsFormularioVisivel = false;
                NovaDemandaNome = string.Empty;
            });
            
            SalvarNovaDemandaCommand = new RelayCommand(SalvarNovaDemanda);

            AbrirFormPipelineCommand = new RelayCommand(() => 
            {
                AtualizarOpcoesEdicao();
                OpcaoEdicaoSelecionada = "➕ Criar Nova Pipeline";
                IsFormularioPipelineVisivel = true;
            });
            
            CancelarFormPipelineCommand = new RelayCommand(() => 
            {
                IsFormularioPipelineVisivel = false;
                NovoPipelineNome = string.Empty;
                NovoPipelinePassos = string.Empty;
            });
            
            SalvarNovoPipelineCommand = new RelayCommand(SalvarNovoPipeline);
            ExcluirPipelineCommand = new RelayCommand(ExcluirPipeline);
        }

        private void AtualizarOpcoesEdicao()
        {
            OpcoesEdicaoPipeline.Clear();
            OpcoesEdicaoPipeline.Add("➕ Criar Nova Pipeline");
            foreach (var p in TiposDePipeline) OpcoesEdicaoPipeline.Add(p);
        }

        private void CarregarDadosPipelineParaEdicao()
        {
            if (IsModoEdicao && _templates.ContainsKey(OpcaoEdicaoSelecionada))
            {
                _pipelineOriginalNome = OpcaoEdicaoSelecionada;
                NovoPipelineNome = OpcaoEdicaoSelecionada;
                NovoPipelinePassos = string.Join("; ", _templates[OpcaoEdicaoSelecionada]);
            }
            else
            {
                _pipelineOriginalNome = string.Empty;
                NovoPipelineNome = string.Empty;
                NovoPipelinePassos = string.Empty;
            }
        }

        private void SalvarNovoPipeline()
        {
            if (string.IsNullOrWhiteSpace(NovoPipelineNome) || string.IsNullOrWhiteSpace(NovoPipelinePassos))
                return;

            var passos = NovoPipelinePassos
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (passos.Count == 0) return;

            if (IsModoEdicao && _pipelineOriginalNome != NovoPipelineNome)
            {
                _templates.Remove(_pipelineOriginalNome);
                TiposDePipeline.Remove(_pipelineOriginalNome);
                
                foreach (var d in DemandasAtivas.Where(x => x.TipoPipeline == _pipelineOriginalNome))
                {
                    d.TipoPipeline = NovoPipelineNome;
                }
                _arquivos.SalvarEstado(DemandasAtivas.ToList());
            }

            _templates[NovoPipelineNome] = passos;
            if (!TiposDePipeline.Contains(NovoPipelineNome)) TiposDePipeline.Add(NovoPipelineNome);

            _arquivos.SalvarTemplates(_templates);
            NovaDemandaPipeline = NovoPipelineNome; 
            AtualizarTextoPassoAtual(); 

            AtualizarOpcoesEdicao();
            OpcaoEdicaoSelecionada = "➕ Criar Nova Pipeline";
        }

        private void ExcluirPipeline()
        {
            if (IsModoEdicao)
            {
                _templates.Remove(_pipelineOriginalNome);
                TiposDePipeline.Remove(_pipelineOriginalNome);
                _arquivos.SalvarTemplates(_templates);
                
                var demandasRemover = DemandasAtivas.Where(x => x.TipoPipeline == _pipelineOriginalNome).ToList();
                foreach (var d in demandasRemover) DemandasAtivas.Remove(d);
                _arquivos.SalvarEstado(DemandasAtivas.ToList());

                if (DemandaSelecionada != null && demandasRemover.Contains(DemandaSelecionada))
                {
                    DemandaSelecionada = DemandasAtivas.FirstOrDefault();
                }

                AtualizarOpcoesEdicao();
                OpcaoEdicaoSelecionada = "➕ Criar Nova Pipeline";
            }
        }

        private void SalvarNovaDemanda()
        {
            // O GRITO DE AVISO
            if (string.IsNullOrWhiteSpace(NovaDemandaNome))
            {
                MessageBox.Show("Você precisa dar um nome para a demanda!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NovaDemandaPipeline)) 
            {
                MessageBox.Show("Por favor, selecione uma opção de pipeline na lista!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var nova = new DemandaAtiva { NomeDemanda = NovaDemandaNome, TipoPipeline = NovaDemandaPipeline, IndicePassoAtual = 0 };
            DemandasAtivas.Add(nova);
            _arquivos.SalvarEstado(DemandasAtivas.ToList());
            DemandaSelecionada = nova; 
            CancelarFormCommand.Execute(null);
        }

        private void AvancarPasso()
        {
            if (DemandaSelecionada == null || !_templates.ContainsKey(DemandaSelecionada.TipoPipeline)) return;

            var passos = _templates[DemandaSelecionada.TipoPipeline];
            DemandaSelecionada.IndicePassoAtual++;

            if (DemandaSelecionada.IndicePassoAtual >= passos.Count)
            {
                DemandasAtivas.Remove(DemandaSelecionada);
                DemandaSelecionada = DemandasAtivas.FirstOrDefault(); // Pula pra próxima

                if (DemandaSelecionada == null)
                {
                    TextoPassoAtual = "Demanda Finalizada! 🎉\nNenhuma outra pendente.";
                }
            }
            else
            {
                AtualizarTextoPassoAtual();
            }

            _arquivos.SalvarEstado(DemandasAtivas.ToList());
        }

        private void AtualizarTextoPassoAtual()
        {
            if (DemandaSelecionada == null)
            {
                TextoPassoAtual = DemandasAtivas.Any() 
                    ? "Selecione uma demanda acima 👆" 
                    : "Tudo limpo! Nenhuma demanda pendente. ☕";
                return;
            }

            if (_templates.TryGetValue(DemandaSelecionada.TipoPipeline, out var passos) && DemandaSelecionada.IndicePassoAtual < passos.Count)
            {
                TextoPassoAtual = passos[DemandaSelecionada.IndicePassoAtual];
            }
        }
    }
}