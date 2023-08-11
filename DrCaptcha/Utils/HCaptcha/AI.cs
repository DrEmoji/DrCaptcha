using Clarifai.Api;
using Clarifai.Channels;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DrCaptcha.Utils.HCaptcha
{
    internal class AI
    {
        static bool FindMatchingSubword(string keyword, string predictedConcepts)
        {
            string[] conceptsArray = predictedConcepts.Split('\n');

            if (Array.Exists(conceptsArray, concept => concept.Contains(keyword)))
            {
                return true;
            }

            foreach (string predictedWord in conceptsArray)
            {
                for (int i = 0; i < predictedWord.Length; i++)
                {
                    for (int j = i + 4; j <= predictedWord.Length; j++)
                    {
                        string subword = predictedWord.Substring(i, j - i);
                        if (subword.Length > 4 && keyword.Contains(subword))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static string[] Recognise(dynamic task, string keyword)
        {
            V2.V2Client client = new V2.V2Client(ClarifaiChannel.Grpc());
            Metadata metadata = new Metadata
             {
                {"Authorization", "Key 48e4bb33c47747378d4c2f9a115aa06f"}
            };
            string imagelink = task["datapoint_uri"].ToString();
            string taskkey = task["task_key"].ToString();
            var response = client.PostModelOutputs(
                new PostModelOutputsRequest()
                {
                    ModelId = "aaa03c23b3724a16a56b629203edc62c", // <- This is the general model_id
                    Inputs =
                    {
                        new List<Input>()
                        {
                            new Input()
                            {
                                Data = new Data()
                                {
                                    Image = new Clarifai.Api.Image()
                                    {
                                        Url = imagelink
                                    }
                                }
                            }
                        }
                    }
                },
                metadata
            );
            foreach (var concept in response.Outputs[0].Data.Concepts)
            {
                bool result = FindMatchingSubword(keyword, concept.Name);
                if (result)
                {
                    return new string[] { taskkey, "true" };
                }
            }
            return new string[] { taskkey, "false" };
        }
    }
}
