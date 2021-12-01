using Domain.Entities;
using Domain.Ports;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    [DomainService]
    public  class ImageSorterService
    {
        private readonly MLContext _context;
        private readonly ITransformer _trainedModel;
        private readonly PredictionEngine<ImageData, ImagePrediction> _engine;
        private readonly IImageSorterRepository _imageSorterRepository;

        public ImageSorterService(MLContext context, IImageSorterRepository imageSorterRepository)
        {
            imageSorterRepository.GenerateModel();
            _context = context;
            var cwp = Path.Combine(Directory.GetCurrentDirectory(), "ImageSorterModel.zip");
            _trainedModel = _context.Model.Load(cwp, out var modelInputSchema);
            _engine = _context.Model.CreatePredictionEngine<ImageData, ImagePrediction>(_trainedModel);
            _imageSorterRepository = imageSorterRepository;

        }

        public ImagePrediction Predict(ImageData imageData)
        {
            return _engine.Predict(imageData);
        }

        public void TrainModel()
        {
            _imageSorterRepository.GenerateModel();
        }

    }
}
