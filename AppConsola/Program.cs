using Domain.Entities;
using Domain.Structs;
using Microsoft.ML;
using Microsoft.ML.Data;

string currentDirectory = Directory.GetCurrentDirectory();
string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\"));
string modelPath = Path.Combine(solutionDirectory, "Api","Data", "ImageSorterModel.zip");
string tsvPath = Path.Combine(solutionDirectory, "Api/Data/assets/tsv/test-tags.tsv");


MLContext mlContext = new MLContext(seed: 0);

// Cargar el modelo guardado
ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelSchema);

IDataView testDataView = mlContext.Data.LoadFromTextFile<ImageData>(path: tsvPath, hasHeader: false);

// Aplicar el modelo a los datos de prueba
var transformedTestData = trainedModel.Transform(testDataView);

// Calcular métricas de evaluación
var metrics = mlContext.MulticlassClassification.Evaluate(transformedTestData, labelColumnName: "LabelKey", predictedLabelColumnName: "PredictedLabel");

// O mostrar las métricas en la consola
Console.WriteLine($"Micro Accuracy: {metrics.MacroAccuracy}");
Console.WriteLine($"Macro Accuracy: {metrics.MicroAccuracy}");
Console.WriteLine($"Confusion Matrix: {metrics.ConfusionMatrix.NumberOfClasses}");
foreach (var item in metrics.ConfusionMatrix.PerClassPrecision)
{
    Console.WriteLine($"Presicions: {item}");
}

foreach (var item in metrics.PerClassLogLoss)
{
    Console.WriteLine($"LogLoss Per Class: {item}");
}

Console.WriteLine($"Log Loss: {metrics.LogLoss}");