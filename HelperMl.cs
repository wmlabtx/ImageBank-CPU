using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;


namespace ImageBank
{
    public static class HelperMl
    {
		private const string model_name = "nasnet_large.pb";

        private static MLContext mlContext;
        private static ITransformer mlModel;

        public static bool Init()
		{
            var modelfilename = Path.Combine(Environment.CurrentDirectory, model_name);
            if (!File.Exists(modelfilename)) {
				return false;
			}

			mlContext = new MLContext();
			var model = mlContext.Model.LoadTensorFlowModel(modelfilename);
            var pipeline =
                mlContext.Transforms
                    .ResizeImages(
                        outputColumnName: "input",
                        imageWidth: 331,
                        imageHeight: 331,
                        inputColumnName: "input")
                    .Append(mlContext.Transforms.ExtractPixels(
                        outputColumnName: "input"))
                    .Append(model.ScoreTensorFlowModel(
                            outputColumnName: "final_layer/dropout/Identity",
                            inputColumnName: "input"))
                    .AppendCacheCheckpoint(mlContext);

            var list = new List<ImageInputData>
            {
                new ImageInputData() { Image = new Bitmap(331, 331) }
            };

            var dv = mlContext.Data.LoadFromEnumerable(list);
            mlModel = pipeline.Fit(dv);

            /*
            if (!Helper.GetImageDataFromFile(
                "org.jpg",
                out var imgdata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out var checksum,
                out var needwrite)) {
                return false;
            }

            if (!Helper.GetVector(bitmap, out var vector)) {
                return false;
            }
            */

            return true;
		}

		public static float[] GetVector(Bitmap bitmap)
		{
            Contract.Requires(bitmap != null);
            var imageInputData = new ImageInputData { Image = bitmap };
            using (var predictor = mlContext.Model.CreatePredictionEngine<ImageInputData, ImageOutputData>(mlModel)) {
                var prediction = predictor.Predict(imageInputData);
                return prediction.Vector;
            }
		}
	}

    public class ImageInputData
    {
        [ImageType(331, 331)]
        [ColumnName("input")]
        public Bitmap Image { get; set; }
    }

    public class ImageOutputData
    {
        [ColumnName("final_layer/dropout/Identity")]
        public float[] Vector;
    }
}
