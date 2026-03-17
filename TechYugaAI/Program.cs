using System.ClientModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using OpenAI;
using TechYugaAI.Components;
using TechYugaAI.Services;
using TechYugaAI.Services.Ingestion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 20 * 1024 * 1024;
});

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Retrieves the token you just set via user-secrets 
var token = builder.Configuration["GitHubModels:Token"] ?? throw new InvalidOperationException("Missing GitHubModels:Token");
var credential = new ApiKeyCredential(token);

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.inference.ai.azure.com")
};

var ghModelsClient = new OpenAIClient(credential, openAIOptions);
var chatClient = ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient();
var embeddingGenerator = ghModelsClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteVectorStore(_ => vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedChunk>(IngestedChunk.CollectionName, vectorStoreConnectionString);

builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));
builder.Services.AddSingleton<CvTemplateRegistry>();
builder.Services.AddScoped<CvDraftState>();
builder.Services.AddScoped<CvPdfGenerator>();
builder.Services.AddScoped<TechYugaAgentTools>();

// Updated: Enables the AI to actually call your C# methods/tools
builder.Services.AddChatClient(chatClient)
    .UseFunctionInvocation()
    .UseLogging();

builder.Services.AddEmbeddingGenerator(embeddingGenerator);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
