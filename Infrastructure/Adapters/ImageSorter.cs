using Domain.Entities;
using Domain.Ports;
using Domain.Structs;
using Microsoft.ML;

namespace Infrastructure.Adapters
{
    public class ImageSorter : IImageSorter
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private const  string BaseDatasetsRelativePath = @"../../../Datos";
        private static string ModelRelativePath = $"{BaseDatasetsRelativePath}/ImageSorterModel.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);
        private static string ImagesFolder = GetAbsolutePath("/assets/images");
        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/tags.tsv";
        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string InceptionTensorFlowModel = Path.Combine(ModelPath, "inception", "tensorflow_inception_graph.pb");


        public void ClassifySingleImage()
        {
            throw new NotImplementedException();
        }

        public void GenerateModel()
        {
            MLContext mlContext = new MLContext(seed: 0);
            IEstimator<ITransformer> pipeline = mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: ImagesFolder, inputColumnName: nameof(ImageData.ImagePath))
                            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
                            .Append(mlContext.Model.LoadTensorFlowModel(InceptionTensorFlowModel).
                                ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey", inputColumnName: "Label"))
                            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelKey", featureColumnName: "softmax2_pre_activation"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"))
                            .AppendCacheCheckpoint(mlContext);

            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<ImageData>(path: TrainDataPath, hasHeader: false);
            ITransformer trainedModel = pipeline.Fit(trainingDataView);
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            string fullPath = Path.Combine(AppPath, relativePath);
            return fullPath;
        }

    }
}
