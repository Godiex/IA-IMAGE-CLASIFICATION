using Domain.Entities;
using Microsoft.ML;

string currentDirectory = Directory.GetCurrentDirectory();
string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\"));
string modelPath = Path.Combine(solutionDirectory, "Api","Data", "ImageSorterModel.zip");
string tsvPath = Path.Combine(solutionDirectory, "Api/Data/assets/tsv/test-tags.tsv");


MLContext mlContext = new MLContext(seed: 0);

ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelSchema);

IDataView testDataView = mlContext.Data.LoadFromTextFile<ImageData>(path: tsvPath, hasHeader: false);

var transformedTestData = trainedModel.Transform(testDataView);

var metrics = mlContext.MulticlassClassification.Evaluate(transformedTestData, labelColumnName: "LabelKey", predictedLabelColumnName: "PredictedLabel");

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

IEnumerable<ImagePrediction> imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(transformedTestData, true);
DisplayResults(imagePredictionData);


void DisplayResults(IEnumerable<ImagePrediction> imagePredictionData)
{
    foreach (ImagePrediction prediction in imagePredictionData)
    {
        Console.WriteLine($"Image: {Path.GetFileName(prediction.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score?.Max()} ");
    }
}