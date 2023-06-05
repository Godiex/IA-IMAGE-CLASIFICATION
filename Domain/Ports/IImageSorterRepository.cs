using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Ports
{
    public interface IImageSorterRepository
    {
        string GetModelPath();
        string GetImageClasificationFolder();
        string GetDataTrainPath();
        void GenerateModel();

    }
}
