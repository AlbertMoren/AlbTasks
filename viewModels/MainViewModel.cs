using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AlbTasks.Models;
using System.Text.Json;
using System.Windows.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlbTasks.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ------------------ CONTROLE DE TELA ------------------
        private bool _isFormularioVisivel;
        public bool IsFormularioVisivel
        {
            get => _isFormularioVisivel;
            set { _isFormularioVisivel = value; OnPropertyChanged(); }
        }

        private bool _isFormularioPipelineVisivel;
        public bool IsFormularioPipelineVisivel
        {
            get => _isFormularioPipelineVisivel;
            set { _isFormularioPipelineVisivel = value; OnPropertyChanged(); }
        }

        private bool _isFormularioDiariaVisivel;
        public bool IsFormularioDiariaVisivel
        {
            get => _isFormularioDiariaVisivel;
            set { _isFormularioDiariaVisivel = value; OnPropertyChanged(); }
        }

        // ------------------ DADOS PRINCIPAIS ------------------
        public ObservableCollection<DemandaAtiva> DemandasAtivas { get; set; } = new();
        
        private DemandaAtiva? _demandaSelecionada;
        public DemandaAtiva? DemandaSelecionada
        {
            get => _demandaSelecionada;
            set { _demandaSelecionada = value; OnPropertyChanged(); AtualizarTextoPassoAtual(); }
        }

        private string _textoPassoAtual = "Selecione ou crie uma demanda";
        public string TextoPassoAtual
        {
            get => _textoPassoAtual;
            set { _textoPassoAtual = value; OnPropertyChanged(); }
        }

        // ------------------ FORMULÁRIO: NOVA DEMANDA ------------------
        private string _novaDemandaNome = string.Empty;
        public string NovaDemandaNome
        {
            get => _novaDemandaNome;
            set { _novaDemandaNome = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> PipelinesDisponiveis { get; set; } = new();
        
        private string? _pipelineSelecionadaParaDemanda;
        public string? PipelineSelecionadaParaDemanda
        {
            get => _pipelineSelecionadaParaDemanda;
            set { _pipelineSelecionadaParaDemanda = value; OnPropertyChanged(); }
        }

        // ------------------ FORMULÁRIO: PIPELINE ------------------
        public ObservableCollection<string> OpcoesEdicaoPipeline { get; set; } = new();
        
        private string? _opcaoEdicaoSelecionada;
        public string? OpcaoEdicaoSelecionada
        {
            get => _opcaoEdicaoSelecionada;
            set { _opcaoEdicaoSelecionada = value; CarregarPipelineParaEdicao(); OnPropertyChanged(); }
        }

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

        private bool _isModoEdicao;
        public bool IsModoEdicao
        {
            get => _isModoEdicao;
            set { _isModoEdicao = value; OnPropertyChanged(); }
        }

        // ------------------ FORMULÁRIO: DIÁRIA (EDIÇÃO E EXCLUSÃO) ------------------
        private string _novaDiariaTexto = string.Empty;
        public string NovaDiariaTexto
        {
            get => _novaDiariaTexto;
            set { _novaDiariaTexto = value; OnPropertyChanged(); }
        }

        private string _novaDiariaHorario = string.Empty;
        public string NovaDiariaHorario
        {
            get => _novaDiariaHorario;
            set { _novaDiariaHorario = value; OnPropertyChanged(); }
        }

        private bool _isModoEdicaoDiaria;
        public bool IsModoEdicaoDiaria
        {
            get => _isModoEdicaoDiaria;
            set { _isModoEdicaoDiaria = value; OnPropertyChanged(); }
        }

        private AtividadeDiaria? _diariaEmEdicao;

        // ------------------ GAVETA DE ALERTAS ------------------
        public ObservableCollection<LembreteViewModel> LembretesAtivos { get; set; } = new();
        
        private bool _isListaLembretesVisivel;
        public bool IsListaLembretesVisivel
        {
            get => _isListaLembretesVisivel;
            set { _isListaLembretesVisivel = value; OnPropertyChanged(); }
        }

        private List<AtividadeDiaria> _todasAsDiarias = new();
        private DispatcherTimer _timerAlertas;

        // ------------------ COMANDOS ------------------
        public ICommand AbrirFormCommand { get; }
        public ICommand CancelarFormCommand { get; }
        public ICommand SalvarNovaDemandaCommand { get; }
        
        public ICommand AbrirFormPipelineCommand { get; }
        public ICommand CancelarFormPipelineCommand { get; }
        public ICommand SalvarNovoPipelineCommand { get; }
        public ICommand ExcluirPipelineCommand { get; }

        public ICommand AbrirFormDiariaCommand { get; }
        public ICommand EditarDiariaCommand { get; }
        public ICommand CancelarFormDiariaCommand { get; }
        public ICommand SalvarNovaDiariaCommand { get; }
        public ICommand ExcluirDiariaCommand { get; }
        public ICommand ConcluirDiariaCommand { get; }

        public ICommand AvancarPassoCommand { get; }
        public ICommand VoltarPassoCommand { get; }

        public MainViewModel()
        {
            AbrirFormCommand = new RelayCommand(_ => AbrirFormularioNovaDemanda());
            CancelarFormCommand = new RelayCommand(_ => IsFormularioVisivel = false);
            SalvarNovaDemandaCommand = new RelayCommand(_ => SalvarDemanda());

            AbrirFormPipelineCommand = new RelayCommand(_ => AbrirFormularioPipeline());
            CancelarFormPipelineCommand = new RelayCommand(_ => IsFormularioPipelineVisivel = false);
            SalvarNovoPipelineCommand = new RelayCommand(_ => SalvarPipeline());
            ExcluirPipelineCommand = new RelayCommand(_ => ExcluirPipeline());

            AbrirFormDiariaCommand = new RelayCommand(_ => AbrirFormularioDiariaNova());
            EditarDiariaCommand = new ParamCommand(param => AbrirFormularioDiariaEdicao(param as AtividadeDiaria));
            CancelarFormDiariaCommand = new RelayCommand(_ => IsFormularioDiariaVisivel = false);
            SalvarNovaDiariaCommand = new RelayCommand(_ => SalvarDiaria());
            ExcluirDiariaCommand = new RelayCommand(_ => ExcluirDiaria());
            ConcluirDiariaCommand = new ParamCommand(param => ConcluirAtividadeDiaria(param as AtividadeDiaria));

            AvancarPassoCommand = new RelayCommand(_ => AvancarPasso());
            VoltarPassoCommand = new RelayCommand(_ => VoltarPasso());

            CarregarDadosIniciais();

            _timerAlertas = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timerAlertas.Tick += (s, e) => AtualizarAlertasDiarios();
            _timerAlertas.Start();
        }

        // ------------------ DEMANDAS E PIPELINES ------------------
        private void AbrirFormularioNovaDemanda()
        {
            NovaDemandaNome = string.Empty;
            AtualizarPipelinesDisponiveis();
            IsFormularioVisivel = true;
        }

        private void AtualizarPipelinesDisponiveis()
        {
            PipelinesDisponiveis.Clear();
            var dict = GerenciadorArquivos.CarregarPipelines();
            foreach (var key in dict.Keys)
                PipelinesDisponiveis.Add(key);
            if (PipelinesDisponiveis.Any())
                PipelineSelecionadaParaDemanda = PipelinesDisponiveis.First();
        }

        private void SalvarDemanda()
        {
            if (string.IsNullOrWhiteSpace(NovaDemandaNome) || string.IsNullOrEmpty(PipelineSelecionadaParaDemanda)) return;

            var dict = GerenciadorArquivos.CarregarPipelines();
            if (dict.TryGetValue(PipelineSelecionadaParaDemanda, out var passos))
            {
                var nova = new DemandaAtiva
                {
                    NomeDemanda = NovaDemandaNome,
                    NomePipeline = PipelineSelecionadaParaDemanda,
                    Passos = passos,
                    IndicePassoAtual = 0
                };
                DemandasAtivas.Add(nova);
                DemandaSelecionada = nova;
                SalvarEstadoAtual();
            }
            IsFormularioVisivel = false;
        }

        private void AbrirFormularioPipeline()
        {
            IsModoEdicao = false;
            NovoPipelineNome = string.Empty;
            NovoPipelinePassos = string.Empty;
            AtualizarOpcoesEdicaoPipeline();
            IsFormularioPipelineVisivel = true;
        }

        private void AtualizarOpcoesEdicaoPipeline()
        {
            OpcoesEdicaoPipeline.Clear();
            OpcoesEdicaoPipeline.Add("+ Criar Novo Pipeline");
            var dict = GerenciadorArquivos.CarregarPipelines();
            foreach (var key in dict.Keys)
                OpcoesEdicaoPipeline.Add(key);
            OpcaoEdicaoSelecionada = OpcoesEdicaoPipeline.First();
        }

        private void CarregarPipelineParaEdicao()
        {
            if (OpcaoEdicaoSelecionada == null || OpcaoEdicaoSelecionada == "+ Criar Novo Pipeline")
            {
                IsModoEdicao = false;
                NovoPipelineNome = string.Empty;
                NovoPipelinePassos = string.Empty;
            }
            else
            {
                IsModoEdicao = true;
                NovoPipelineNome = OpcaoEdicaoSelecionada;
                var dict = GerenciadorArquivos.CarregarPipelines();
                if (dict.TryGetValue(OpcaoEdicaoSelecionada, out var passos))
                {
                    NovoPipelinePassos = string.Join("; ", passos);
                }
            }
        }

        private void SalvarPipeline()
        {
            if (string.IsNullOrWhiteSpace(NovoPipelineNome) || string.IsNullOrWhiteSpace(NovoPipelinePassos)) return;

            var listaPassos = NovoPipelinePassos.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            var dict = GerenciadorArquivos.CarregarPipelines();
            
            dict[NovoPipelineNome] = listaPassos;
            GerenciadorArquivos.SalvarPipelines(dict);
            IsFormularioPipelineVisivel = false;
        }

        private void ExcluirPipeline()
        {
            if (!IsModoEdicao || string.IsNullOrEmpty(NovoPipelineNome)) return;

            var dict = GerenciadorArquivos.CarregarPipelines();
            if (dict.ContainsKey(NovoPipelineNome))
            {
                dict.Remove(NovoPipelineNome);
                GerenciadorArquivos.SalvarPipelines(dict);
            }
            IsFormularioPipelineVisivel = false;
        }

        // ------------------ DIÁRIAS (EDIÇÃO E EXCLUSÃO) ------------------
        private void AbrirFormularioDiariaNova()
        {
            IsModoEdicaoDiaria = false;
            _diariaEmEdicao = null;
            NovaDiariaTexto = string.Empty;
            NovaDiariaHorario = DateTime.Now.AddHours(1).ToString("HH:mm");
            IsFormularioDiariaVisivel = true;
        }

        private void AbrirFormularioDiariaEdicao(AtividadeDiaria? atividade)
        {
            if (atividade == null) return;
            IsModoEdicaoDiaria = true;
            _diariaEmEdicao = atividade;
            NovaDiariaTexto = atividade.Texto;
            NovaDiariaHorario = atividade.HorarioAlerta.ToString("HH:mm");
            IsFormularioDiariaVisivel = true;
        }

        private void SalvarDiaria()
        {
            if (string.IsNullOrWhiteSpace(NovaDiariaTexto) || !TimeSpan.TryParse(NovaDiariaHorario, out var ts)) return;

            var horarioAlerta = DateTime.Today.Add(ts);

            if (IsModoEdicaoDiaria && _diariaEmEdicao != null)
            {
                _diariaEmEdicao.Texto = NovaDiariaTexto;
                _diariaEmEdicao.HorarioAlerta = horarioAlerta;
            }
            else
            {
                var nova = new AtividadeDiaria
                {
                    Texto = NovaDiariaTexto,
                    HorarioAlerta = horarioAlerta,
                    Concluida = false
                };
                _todasAsDiarias.Add(nova);
            }

            SalvarDiariasNoArquivo();
            AtualizarAlertasDiarios();
            IsFormularioDiariaVisivel = false;
        }

        private void ExcluirDiaria()
        {
            if (!IsModoEdicaoDiaria || _diariaEmEdicao == null) return;

            _todasAsDiarias.Remove(_diariaEmEdicao);
            SalvarDiariasNoArquivo();
            AtualizarAlertasDiarios();
            IsFormularioDiariaVisivel = false;
        }

        private void ConcluirAtividadeDiaria(AtividadeDiaria? atividade)
        {
            if (atividade == null) return;
            atividade.Concluida = true;
            SalvarDiariasNoArquivo();
            AtualizarAlertasDiarios();
        }

        private void AtualizarAlertasDiarios()
        {
            LembretesAtivos.Clear();
            var agora = DateTime.Now;

            foreach (var d in _todasAsDiarias.Where(x => !x.Concluida))
            {
                var diff = d.HorarioAlerta - agora;
                int nivel = 0;

                if (diff.TotalMinutes <= 15 && diff.TotalMinutes > 5)
                    nivel = 1; // Alerta
                else if (diff.TotalMinutes <= 5)
                    nivel = 2; // Urgente

                LembretesAtivos.Add(new LembreteViewModel
                {
                    Texto = $"{d.Texto} ({d.HorarioAlerta:HH:mm})",
                    NivelUrgencia = nivel,
                    AtividadeOriginal = d
                });
            }

            IsListaLembretesVisivel = LembretesAtivos.Any();
        }

        // ------------------ NAVEGAÇÃO DOS PASSOS ------------------
        private void AvancarPasso()
        {
            if (DemandaSelecionada == null) return;
            if (DemandaSelecionada.IndicePassoAtual < DemandaSelecionada.Passos.Count - 1)
            {
                DemandaSelecionada.IndicePassoAtual++;
                AtualizarTextoPassoAtual();
                SalvarEstadoAtual();
            }
            else
            {
                DemandasAtivas.Remove(DemandaSelecionada);
                DemandaSelecionada = DemandasAtivas.FirstOrDefault();
                SalvarEstadoAtual();
            }
        }

        private void VoltarPasso()
        {
            if (DemandaSelecionada == null) return;
            if (DemandaSelecionada.IndicePassoAtual > 0)
            {
                DemandaSelecionada.IndicePassoAtual--;
                AtualizarTextoPassoAtual();
                SalvarEstadoAtual();
            }
        }

        private void AtualizarTextoPassoAtual()
        {
            if (DemandaSelecionada != null && DemandaSelecionada.Passos.Any())
            {
                TextoPassoAtual = DemandaSelecionada.Passos[DemandaSelecionada.IndicePassoAtual];
            }
            else
            {
                TextoPassoAtual = "Nenhuma demanda ativa";
            }
        }

        // ------------------ PERSISTÊNCIA ------------------
        private void SalvarEstadoAtual()
        {
            try
            {
                var json = JsonSerializer.Serialize(DemandasAtivas);
                File.WriteAllText("estado_atual.json", json);
            }
            catch { }
        }

        private void SalvarDiariasNoArquivo()
        {
            try
            {
                var json = JsonSerializer.Serialize(_todasAsDiarias);
                File.WriteAllText("diarias.json", json);
            }
            catch { }
        }

        private void CarregarDadosIniciais()
        {
            try
            {
                if (File.Exists("estado_atual.json"))
                {
                    var json = File.ReadAllText("estado_atual.json");
                    var lista = JsonSerializer.Deserialize<List<DemandaAtiva>>(json);
                    if (lista != null)
                    {
                        foreach (var d in lista) DemandasAtivas.Add(d);
                        DemandaSelecionada = DemandasAtivas.FirstOrDefault();
                    }
                }

                if (File.Exists("diarias.json"))
                {
                    var json = File.ReadAllText("diarias.json");
                    var lista = JsonSerializer.Deserialize<List<AtividadeDiaria>>(json);
                    if (lista != null)
                    {
                        _todasAsDiarias = lista;
                    }
                }
            }
            catch { }
            AtualizarAlertasDiarios();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ParamCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public ParamCommand(Action<object?> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
    }

    public class LembreteViewModel
    {
        public string Texto { get; set; } = string.Empty;
        public int NivelUrgencia { get; set; }
        public AtividadeDiaria AtividadeOriginal { get; set; } = new();
    }
}