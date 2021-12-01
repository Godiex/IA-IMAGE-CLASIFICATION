using Domain.Entities;
using Domain.Ports;
using Domain.Structs;
using Microsoft.ML;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Adapters
{
    public class ImageSorterRepository : IImageSorterRepository
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private string BasePath = default!;
        public string ModelPath { get; set; } = default!;
        private string ImagesFolder = default!;
        private string InceptionTensorFlowModel = default!;
        private string TrainDataPath = default!;

        public ImageSorterRepository(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            BasePath = Path.Combine($"{hostingEnvironment.ContentRootPath}", "Data");
            SetPathsOfModel();
        }

        private void SetPathsOfModel()
        {
            if (BasePath != null) 
            {
                ModelPath = GetAbsolutePath("ImageSorterModel.zip");
                ImagesFolder = GetAbsolutePath("assets/images");
                TrainDataPath = GetAbsolutePath("assets/tsv/tags.tsv");
                InceptionTensorFlowModel = GetAbsolutePath("assets/inception/tensorflow_inception_graph.pb");

            }
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

        private string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(BasePath, relativePath);
        }

        public string GetModelPath()
        {
            return ModelPath;
        }

        public string GetImageClasificationFolder()
        {
            return ImagesFolder;
        }

    }
}
