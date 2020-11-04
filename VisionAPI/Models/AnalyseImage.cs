using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace EndgameVision.Models
{
    public class AnalyseImage
    {
        public string Description { get; set; }
        public string Adult { get; set; }
        public string Categories { get; set; }
        public string Tags { get; set; }

        private const string subscriptionKey = "1a20142b75eb46c58c80ee7e12c1f45e";

        // Specify the features to return
        private static readonly List<VisualFeatureTypes> features =
            new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult
            };

        public async Task Initial(string remoteUrl)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });

            // Specify the Azure region
            computerVision.Endpoint = "https://compvisiontech.cognitiveservices.azure.com/";

            await AnalyzeRemote(computerVision, remoteUrl);
        }

        // Analyze a remote image
        public async Task AnalyzeRemote(
            ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine(
                    "\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
                return;
            }

            ImageAnalysis analysis = await computerVision.AnalyzeImageAsync(imageUrl, features);
            Description = getDescription(analysis, imageUrl);
            Adult = getAdult(analysis, imageUrl);
            Categories = getCategories(analysis, imageUrl);
            Tags = getTags(analysis, imageUrl);
        }

        public string getDescription(ImageAnalysis analysis, string imageUri)
        {
            string description = "";

            if (analysis.Description.Captions.Count != 0)
            {
                description = analysis.Description.Captions[0].Text;
            }
            else
            {
                description = "No description generated.";
            }

            return description;
        }

        public string getAdult(ImageAnalysis analysis, string imageUri)
        {
            string adult = "";

            if (analysis.Adult.IsAdultContent)
            {
                adult = "Contains Adult Content";
            }
            else
            {
                adult = "Contains no Adult Content";
            }

            return adult;
        }

        public string getCategories(ImageAnalysis analysis, string imageUri)
        {
            string categories = "";

            if (analysis.Categories.Count != 0)
            {
                categories = analysis.Categories[0].Name;
            }
            else
            {
                categories = "No categories found.";
            }

            return categories;
        }

        public string getTags(ImageAnalysis analysis, string imageUri)
        {
            string tags = "";

            if (analysis.Tags.Count > 3)
            {
                tags = analysis.Tags[0].Name + ", " +
                       analysis.Tags[1].Name + ", " +
                       analysis.Tags[2].Name;
            }
            else
            {
                tags = "No tags found.";
            }

            return tags;
        }
    }
}