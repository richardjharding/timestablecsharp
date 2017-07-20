using AlexaSkillsKit.Slu;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaSkillsKit.Authentication;
using AlexaSkillsKit.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionApp1
{
    public class TimesTableSpeechlet : Speechlet
    {
        private const string SELECTED_TABLE = "selectedTable";
        private const string SELECTED_MULTIPLIER = "selectedMultipler";
        private const string QUESTIONS_ASKED = "questionsAsked";
        private const string CORRECT_ANSWERS = "correctAnswers";
        private TraceWriter log;

        public TimesTableSpeechlet(TraceWriter log)
        {
            this.log = log;
        }

        public override SpeechletResponse OnIntent(IntentRequest intentRequest, Session session)
        {
            Intent intent = intentRequest.Intent;
            string intentName = (intent != null) ? intent.Name : null;

            switch (intentName)
            {
                case "AMAZON.CancelIntent":
                    log.Info("Cancel Intent detected");
                    var cancelResponse = "";
                    if(session.Attributes.ContainsKey(CORRECT_ANSWERS) && session.Attributes.ContainsKey(QUESTIONS_ASKED))
                    {
                        var questionsAsked = int.Parse(session.Attributes[QUESTIONS_ASKED]) -1;
                        var correctAnswers = int.Parse(session.Attributes[CORRECT_ANSWERS]);
                        cancelResponse = string.Format("thanks for playing you. scored a total of {0} out of {1}", correctAnswers,questionsAsked);
                    }
                    else
                    {
                        cancelResponse = "Ok maybe another time";
                    }                    

                    return BuildSpeechletResponse("Cancel", cancelResponse, true);
                case "AMAZON.StopIntent":
                    log.Info("Stop intent detected");
                    var stopResponse = "";
                    if (session.Attributes.ContainsKey(CORRECT_ANSWERS) && session.Attributes.ContainsKey(QUESTIONS_ASKED))
                    {
                        var questionsAsked = int.Parse(session.Attributes[QUESTIONS_ASKED]) - 1;
                        var correctAnswers = int.Parse(session.Attributes[CORRECT_ANSWERS]);
                        stopResponse = string.Format("thanks for playing. you scored a total of {0} out of {1}", correctAnswers, questionsAsked);
                    }
                    else
                    {
                        stopResponse = "Ok maybe another time";
                    }
                    return BuildSpeechletResponse("Bye", stopResponse, true);
                case "BeginQuiz":
                    log.Info("BeginQuiz intent detected");
                    if(intent.Slots.Keys.Contains("TestTable") && !string.IsNullOrEmpty( intent.Slots["TestTable"].Value))
                    {
                        var table = intent.Slots["TestTable"].Value;
                        var question = GenerateQuestion(int.Parse(table));
                        session.Attributes.Clear();
                        session.Attributes[SELECTED_TABLE] = question.Table.ToString();
                        session.Attributes[SELECTED_MULTIPLIER] = question.Multiplier.ToString();
                        session.Attributes[QUESTIONS_ASKED] = 1.ToString();
                        session.Attributes[CORRECT_ANSWERS] = 0.ToString();
                        // TODO keep track of questions asked - and time taken?
                        
                        return BuildSpeechletResponse("Begin", string.Format("Testing you on your {0} times table. What is {1} times {2}", table, question.Table, question.Multiplier), false);
                    }
                    else
                    {
                        return BuildSpeechletResponse("Begin", "Would you like to play a game?", false);
                    }
                case "GuessNumber":
                    log.Info("GuessNumber intent detected");
                    if (intent.Slots.Keys.Contains("Guess") && !string.IsNullOrEmpty(intent.Slots["Guess"].Value))
                    {
                        string response = "";
                        if (session.Attributes.Keys.Contains(SELECTED_TABLE) && session.Attributes.Keys.Contains(SELECTED_MULTIPLIER))
                        {
                            // we have a question and a guess
                            var question = new TimesTableQuestion(int.Parse(session.Attributes[SELECTED_TABLE]), int.Parse(session.Attributes[SELECTED_MULTIPLIER]));
                            var answer = int.Parse(intent.Slots["Guess"].Value);
                            var nextQuestion = new TimesTableQuestion(int.Parse(session.Attributes[SELECTED_TABLE]));
                            
                            session.Attributes[SELECTED_TABLE] = nextQuestion.Table.ToString();
                            session.Attributes[SELECTED_MULTIPLIER] = nextQuestion.Multiplier.ToString();
                            var questionsAsked = int.Parse(session.Attributes[QUESTIONS_ASKED]);
                            var correctAnswers = int.Parse(session.Attributes[CORRECT_ANSWERS]);
                            

                            if (answer == question.Answer)
                            {
                                session.Attributes[CORRECT_ANSWERS] = (++correctAnswers).ToString();
                                session.Attributes[QUESTIONS_ASKED] = (++questionsAsked).ToString();
                                response = string.Format("You guessed {0} which is correct, next question is What is {1} times {2}", answer, nextQuestion.Table, nextQuestion.Multiplier);
                            }
                            else
                            {                                
                                session.Attributes[QUESTIONS_ASKED] = (++questionsAsked).ToString();
                                response = string.Format("You guessed {0} which is incorrect, next question is What is {1} times {2}", answer, nextQuestion.Table, nextQuestion.Multiplier);
                            }

                        }

                        session.Attributes.Add("LastGuess", intent.Slots["Guess"].Value);
                        return BuildSpeechletResponse("Guess", response, false);
                        
                    }
                    else
                    {
                        return BuildSpeechletResponse("Guess", "I didn't quite catch that could you repeat? ", false);
                    }
                    
                default:
                    log.Info("Not intent found");
                    throw new SpeechletException("Invalid intent");
            }
        }

        public override SpeechletResponse OnLaunch(LaunchRequest launchRequest, Session session)
        {
            return BuildSpeechletResponse("Testing", "Hello I can test you on your times tables, which table would you like me to test you on?", false);
        }
       

        public override void OnSessionEnded(SessionEndedRequest sessionEndedRequest, Session session)
        {
            
        }

        public override void OnSessionStarted(SessionStartedRequest sessionStartedRequest, Session session)
        {
            
        }

        public override bool OnRequestValidation(SpeechletRequestValidationResult result, DateTime referenceTimeUtc, SpeechletRequestEnvelope requestEnvelope)
        {
            //return true;
            return base.OnRequestValidation(result, referenceTimeUtc, requestEnvelope);
        }

        public override string ProcessRequest(JObject requestJson)
        {
            var foo = base.ProcessRequest(requestJson);
            return foo;
        }

        public override HttpResponseMessage GetResponse(HttpRequestMessage httpRequest)
        {
            return base.GetResponse(httpRequest);
        }

        public override string ProcessRequest(string requestContent)
        {
            return base.ProcessRequest(requestContent);
        }

        private SpeechletResponse BuildSpeechletResponse(string title, string output, bool shouldEndSession)
        {
            // Create the Simple card content.
            SimpleCard card = new SimpleCard();
            card.Title = title;
            TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;
            card.Content = textInfo.ToTitleCase(output);

            // Create the plain text output.
            PlainTextOutputSpeech speech = new PlainTextOutputSpeech();
            speech.Text = output;

            // Create the speechlet response.
            SpeechletResponse response = new SpeechletResponse();
            response.ShouldEndSession = shouldEndSession;
            response.OutputSpeech = speech;
            response.Card = card;
            Reprompt reprompt = new Reprompt() { OutputSpeech = speech };
            response.Reprompt = reprompt;
            return response;
        }

        private TimesTableQuestion GenerateQuestion(int table)
        {
            Random rnd = new Random();
            var multiplier = rnd.Next(1, 13);
            var question = new TimesTableQuestion(table, multiplier);    
            
            return question;

        }
    }
}
