# AlbTasks 

Um gerenciador de demandas minimalista para Windows, focado em reduzir a carga mental durante o fluxo de trabalho. Em vez de exibir listas gigantes de afazeres, o AlbTasks organiza suas atividades em pipelines padronizados e exibe **apenas o passo atual**, ajudando você a manter o foco absoluto no que precisa ser feito agora.

## Funcionalidades

* **Interface Flutuante (Always on Top):** O aplicativo atua como um widget discreto no centro da tela, sempre visível sobre outras janelas, ocupando o mínimo de espaço possível (autoajustável ao conteúdo).
* **Gestão de Tarefas Diárias:** Acompanhe lembretes e demandas do dia a dia diretamente na janela principal.
* **Pipelines Customizáveis:** Crie, edite e exclua seus próprios fluxos de trabalho. O cadastro é rápido e feito em texto corrido (separando as etapas por `;`).
* **Troca Rápida de Contexto:** Alternância instantânea entre múltiplas tarefas ativas através de "cards" visuais clicáveis, abandonando os menus suspensos tradicionais.
* **Foco e Agilidade:** Avance para o próximo passo da sua demanda com um único clique ou simplesmente pressionando `Enter`.
* **Proteção contra Erros:** Sistema de alertas integrado que impede o salvamento de tarefas incompletas ou sem contexto.
* **Armazenamento Local:** Dados mantidos em arquivos leves `.json` (`pipelines.json`, `diarias.json` e `estado_atual.json`). Zero necessidade de bancos de dados complexos.

## Tecnologias Utilizadas

* **Linguagem:** C# .NET 8.0
* **Interface Gráfica:** Windows Presentation Foundation (WPF)
* **Arquitetura:** Padrão MVVM (Model-View-ViewModel) purista (sem code-behind acoplado), com interface modularizada em sub-views para fácil manutenção.

## Como Compilar e Usar

O AlbTasks foi projetado para rodar de forma independente, sem necessidade de instalação complexa (Single-File Self-Contained).

### 1. Gerando o Executável de Release
Para gerar a versão final otimizada para Windows (64 bits), abra o terminal na raiz do projeto e execute:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

O executável AlbTasks.exe será gerado na pasta

```bash
bin\Release\net8.0-windows\win-x64\publish\
```

### 2. Inicialização Automática com o Windows
Para que o AlbTasks inicie automaticamente como um widget de produtividade:

1. Pressione Win + R e digite shell:startup.

2. Vá até a pasta onde está o seu AlbTasks.exe gerado no passo anterior.

3. Clique com o botão direito no arquivo e selecione Criar atalho.

4. Mova esse atalho gerado para dentro da pasta startup que você abriu.

**Desenvolvido por:** [AlbertMoren](https://github.com/AlbertMoren).