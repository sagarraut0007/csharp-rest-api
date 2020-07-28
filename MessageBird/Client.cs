﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessageBird.Net;
using MessageBird.Net.ProxyConfigurationInjector;
using MessageBird.Objects;
using MessageBird.Objects.Voice;
using MessageBird.Resources;
using MessageBird.Resources.Voice;
using MessageBird.Utilities;

namespace MessageBird
{
    public partial class Client
    {
        public enum Features
        {
            EnableWhatsAppSandboxConversations = 1
        }

        private readonly IRestClient restClient;
        private readonly Features[] features;

        private bool useConversationsWhatsAppSandbox;

        private Client(IRestClient restClient, Features[] features = null)
        {
            this.restClient = restClient;
            this.features = features;
            if (this.features != null && Array.IndexOf(this.features, Features.EnableWhatsAppSandboxConversations) >= 0)
            {
                this.useConversationsWhatsAppSandbox = true;
            }
        }

        public static Client Create(IRestClient restClient, Features[] features = null)
        {
            ParameterValidator.IsNotNull(restClient, "restClient");

            return new Client(restClient, features);
        }

        public static Client CreateDefault(string accessKey, IProxyConfigurationInjector proxyConfigurationInjector = null, Features[] features = null)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(accessKey, "accessKey");

            return new Client(new RestClient(accessKey, proxyConfigurationInjector), features);
        }

        #region Programmable SMS API

        public async Task<Message> SendMessage(string originator, string body, long[] msisdns, MessageOptionalArguments optionalArguments = null)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(originator, "originator");
            ParameterValidator.IsNotNullOrWhiteSpace(body, "body");
            ParameterValidator.ContainsAtLeast(msisdns, 1, "msisdns");

            if (optionalArguments != null)
            {
                ParameterValidator.IsValidMessageType(optionalArguments.Type);
            }

            var recipients = new Recipients(msisdns);
            var message = new Message(originator, body, recipients, optionalArguments);

            var messages = new Messages(message);
            var result = await restClient.Create(messages);

            return result.Object as Message;
        }

        public async Task<Message> ViewMessage(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var messageToView = new Messages(new Message(id));
            var result = await restClient.Retrieve(messageToView);

            return result.Object as Message;
        }

        public async Task<MessageList> ListMessages(string status = "", long recipient=0, DateTime? from=null, DateTime? until=null, int limit = 20, int offset = 0)
        {
            var messageLists = new MessageLists();
            var messageList = new MessageLists(new MessageList { Limit = limit, Offset = offset, Status = status, Recipient=recipient,From=from,Until=until });
            await restClient.Retrieve(messageList);
            return messageList.Object as MessageList;
        }

        #endregion

        #region Programmable Voice API

        /// <summary>
        /// This request retrieves a listing of all call flows.
        /// </summary>
        /// <param name="limit">Set how many records will return from the server</param>
        /// <param name="page">Identify the starting point to return rows from a result</param>
        /// <returns>If successful, this request will return an object with a data, _links and pagination properties.</returns>
        public async Task<CallFlowList> ListCallFlows(int limit = 20, int page = 0)
        {
            var resource = new CallFlowLists(new CallFlowList { Limit = limit, Page = page });

            var result = await restClient.Retrieve(resource);

            return (CallFlowList)result.Object;
        }

        /// <summary>
        /// This request retrieves a call flow resource.<para />
        /// The single parameter is the unique ID that was returned upon creation. 
        /// </summary>
        /// <param name="id">The unique ID which was returned upon creation of a call flow.</param>
        /// <returns></returns>
        public async Task<VoiceResponse<CallFlow>> ViewCallFlow(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var resource = new CallFlows(new CallFlow() { Id = id });
            var result = await restClient.Retrieve(resource);

            return (VoiceResponse<CallFlow>)result.Object;
        }

        /// <summary>
        /// Creating a call flow
        /// </summary>
        /// <param name="request"></param>
        /// <returns>If successful, this request will return an object with a data property, which is an array that has a single call flow object. If the request failed, an error object will be returned.</returns>
        public async Task<VoiceResponse<CallFlow>> CreateCallFlow(CallFlow request)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(request.Title, "title");
            ParameterValidator.IsNotNull(request.Steps, "steps");

            var callFlows = new CallFlows(new CallFlow { Title = request.Title, Steps = request.Steps, Record = request.Record });
            var result = await restClient.Create(callFlows);

            return (VoiceResponse<CallFlow>)result.Object;
        }

        /// <summary>
        /// This request deletes a call flow. The single parameter is the unique ID that was returned upon creation.<br/>
        /// If successful, this request will return an HTTP header of 204 No Content and an empty response. If the request failed, an error object will be returned.
        /// </summary>
        /// <param name="id">The unique ID which was returned upon creation of a call flow.</param>
        public void DeleteCallFlow(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var callFlows = new CallFlows(new CallFlow { Id = id });

            restClient.Delete(callFlows);
        }

        /// <summary>
        /// This request updates a call flow resource. The single parameter is the unique ID that was returned upon creation.<br/>
        /// If successful, this request will return an object with a data property, which is an array that has a single call flow object. If the request failed, an error object will be returned.
        /// </summary>
        /// <param name="id">The unique ID which was returned upon creation of a call flow.</param>
        /// <param name="callFlow"></param>
        /// <returns></returns>
        public async Task<VoiceResponse<CallFlow>> UpdateCallFlow(string id, CallFlow callFlow)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callFlow.Title, "title");
            ParameterValidator.IsNotNull(callFlow.Steps, "steps");

            var callFlows = new CallFlows(new CallFlow { Id = id, Title = callFlow.Title, Steps = callFlow.Steps, Record = callFlow.Record });
            var result = await restClient.Update(callFlows);

            return (VoiceResponse<CallFlow>)result.Object;
        }


        /// <summary>                                                                                                                                                                                                                                                                               
        /// Creating a call
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<VoiceResponse<Call>> CreateCall(Call request)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(request.Source, "source");
            ParameterValidator.IsNotNullOrWhiteSpace(request.Destination, "destination");
            ParameterValidator.IsNotNull(request.CallFlow, "callFlow");

            var callResource = new Calls(request);
            var result = await restClient.Create(callResource);


            return (VoiceResponse<Call>)result.Object;
        }


        /// <summary>
        /// This request retrieves a listing of all calls.
        /// If successful, this request returns an object with a data property, which is an array that has 0 or more recording objects.
        /// </summary>
        /// <param name="limit">Set how many records will return from the server</param>
        /// <param name="page">Identify the starting point to return rows from a result</param>
        /// <returns>If successful, this request will return an object with a data, _links and pagination properties.</returns>
        public async Task<CallList> ListCalls(int limit = 20, int page = 0)
        {
            var resource = new CallLists(new CallList { Limit = limit, Page = page });
            var result = await restClient.Retrieve(resource);
            return (CallList)result.Object;
        }

        public async Task<VoiceResponse<Call>> ViewCall(string callId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");

            var resource = new Calls(new Call { Id = callId });
            var result = await restClient.Retrieve(resource);

            return (VoiceResponse<Call>)result.Object;
        }

        /// <summary>
        /// This request deletes a call. The parameters are the unique ID of the call, the leg and the call with which the call is associated.
        /// If successful, this request will return an HTTP header of 204 No Content and an empty response.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        public void DeleteCall(string callId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");

            var resource = new Calls(new Call { Id = callId });

            restClient.Delete(resource);
        }

        /// <summary>
        /// This request retrieves a listing of all recordings from a specific leg.
        /// If successful, this request returns an object with a data property, which is an array that has 0 or more recording objects.
        /// </summary>
        /// <param name="limit">Set how many records will return from the server</param>
        /// <param name="page">Identify the starting point to return rows from a result</param>
        /// <returns>If successful, this request will return an object with a data, _links and pagination properties.</returns>
        public async Task<RecordingList> ListRecordings(string callId, string legId, int limit = 20, int page = 0)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            var resource = new RecordingLists(new RecordingList { Limit = limit, Page = page, CallId = callId, LegId = legId });
            var result = await restClient.Retrieve(resource);

            return (RecordingList)result.Object;
        }

        /// <summary>
        /// This request retrieves a recording resource. The parameters are the unique ID of the recording, the leg and the call with which the recording is associated.
        /// If successful, this request returns an object with a data property, which is an array that has a single recording object.
        /// If the request failed, an error object will be returned.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        /// <returns>VoiceResponse -Recording-</returns>
        public async Task<VoiceResponse<Recording>> ViewRecording(string callId, string legId, string recordingId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");

            var resource = new Recordings(new Recording { CallId = callId, LegId = legId, Id = recordingId });
            var result = await restClient.Retrieve(resource);

            return (VoiceResponse<Recording>)result.Object;
        }

        /// <summary>
        /// This request deletes a recording. The parameters are the unique ID of the recording, the leg and the call with which the recording is associated.
        /// If successful, this request will return an HTTP header of 204 No Content and an empty response.
        /// If the request failed, an error object will be returned.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        public void DeleteRecording(string callId, string legId, string recordingId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");

            var resource = new Recordings(new Recording { CallId = callId, LegId = legId, Id = recordingId });

            restClient.Delete(resource);
        }

        /// <summary>
        /// The file HATEOAS link has the appropriate URI for downloading a wave file for the recording.
        /// The file is accessible only if you provide the correct API access key for your account and the recording is for a leg/call in your account.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        public Stream DownloadRecording(string callId, string legId, string recordingId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");

            var resource = new Recordings(new Recording { CallId = callId, LegId = legId, Id = recordingId });

            return restClient.PerformHttpRequest(resource.DownloadUri, HttpStatusCode.OK, resource.BaseUrl);
        }

        /// <summary>
        /// This request creates a transcription.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        /// <param name="language">The language of the recording that is to be transcribed.</param>
        /// <returns>If successful, this request will return an object with a data property, which is an array that has a single transcription object. If the request failed, an error object will be returned.</returns>
        public async Task<VoiceResponse<Transcription>> CreateTranscription(string callId, string legId, string recordingId, string language)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");
            ParameterValidator.IsNotNullOrWhiteSpace(language, "language");

            var resource = new Transcriptions(new Transcription { CallId = callId, LegId = legId, RecordingId = recordingId, Language = language });
            var result = await restClient.Create(resource);

            return (VoiceResponse<Transcription>)result.Object;
        }

        /// <summary>
        /// This request retrieves a listing of all transcriptions from a specific recording.
        /// If successful, this request returns an object with a data property, which is an array that has 0 or more transcription objects.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        /// <param name="limit">Set how many records will return from the server</param>
        /// <param name="page">Identify the starting point to return rows from a result</param>
        /// <returns>If successful, this request will return an object with a data, _links and pagination properties.</returns>
        public async Task<TranscriptionList> ListTranscriptions(string callId, string legId, string recordingId, int limit = 20, int page = 0)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");

            var resource = new TranscriptionsLists(new TranscriptionList { Limit = limit, Page = page, CallId = callId, LegId = legId, RecordingId = recordingId });
            var result = await restClient.Retrieve(resource);

            return (TranscriptionList)result.Object;
        }

        /// <summary>
        /// This request retrieves a transcription resource. The parameters are the unique ID of the transcription, the recording, the leg and the call with which the transcription is associated.
        /// If successful, this request returns an object with a data property, which is an array that has a single transcription object.
        /// If the request failed, an error object will be returned.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        /// /// <param name="transcriptionId">The unique ID of a transcription generated upon creation.</param>
        /// <returns>VoiceResponse -Recording-</returns>
        public async Task<VoiceResponse<Transcription>> ViewTranscription(string callId, string legId, string recordingId, string transcriptionId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");
            ParameterValidator.IsNotNullOrWhiteSpace(transcriptionId, "transcriptionId");

            var resource = new Transcriptions(new Transcription { CallId = callId, LegId = legId, RecordingId = recordingId, Id = transcriptionId });
            var result = await restClient.Retrieve(resource);

            return (VoiceResponse<Transcription>)result.Object;
        }

        /// <summary>
        /// The file HATEOAS link has the appropriate URI for downloading a text file for the transcription.
        /// The file is accessible only if you provide the correct API access key for your account and the transcription is for a recording/leg/call in your account.
        /// </summary>
        /// <param name="callId">The unique ID of a call generated upon creation.</param>
        /// <param name="legId">The unique ID of a leg generated upon creation.</param>
        /// <param name="recordingId">The unique ID of a recording generated upon creation.</param>
        /// <param name="transcriptionId">The unique ID of a transcription generated upon creation.</param>
        public Stream DownloadTranscription(string callId, string legId, string recordingId, string transcriptionId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(callId, "callId");
            ParameterValidator.IsNotNullOrWhiteSpace(legId, "legId");
            ParameterValidator.IsNotNullOrWhiteSpace(recordingId, "recordingId");
            ParameterValidator.IsNotNullOrWhiteSpace(transcriptionId, "transcriptionId");

            var resource = new Transcriptions(new Transcription { CallId = callId, LegId = legId, RecordingId = recordingId, Id = transcriptionId });
            return restClient.PerformHttpRequest(resource.DownloadUri, HttpStatusCode.OK, resource.BaseUrl);
        }

        /// <summary>
        /// This request retrieves a listing of all webhooks.
        /// If successful, this request returns an object with a data property, which is an array that has 0 or more recording objects.
        /// </summary>
        /// <param name="limit">Set how many records will return from the server</param>
        /// <param name="page">Identify the starting point to return rows from a result</param>
        /// <returns>If successful, this request will return an object with a data, _links and pagination properties.</returns>
        public async Task<WebhookList> ListWebhooks(int limit = 20, int page = 0)
        {
            var resource = new WebhookLists(new WebhookList { Limit = limit, Page = page });
            var result = await restClient.Retrieve(resource);
            return (WebhookList)result.Object;
        }

        /// <summary>
        /// Create a Webhook
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<VoiceResponse<Webhook>> CreateWebhook(Webhook request)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(request.url, "url");

            var webhookResource = new Webhooks(request);
            var result = await restClient.Create(webhookResource);

            return (VoiceResponse<Webhook>)result.Object;
        }

        /// <summary>
        /// This request retrieves a Webhook
        /// </summary>
        /// <param name="webhookId"></param>Unique identifier of the webhook
        /// <returns></returns>
        public async Task<VoiceResponse<Webhook>> ViewWebhook(string webhookId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(webhookId, "webhookId");

            var resource = new Webhooks(new Webhook { Id = webhookId });
            var result = await restClient.Retrieve(resource);

            return (VoiceResponse<Webhook>)result.Object;
        }

        /// <summary>
        /// This request updates a webhook resource. The single parameter is the unique ID that was returned upon creation.<br/>
        /// If successful, this request will return an object with a data property, which is an array that has a single call flow object. If the request failed, an error object will be returned.
        /// </summary>
        /// <param name="id">The unique ID which was returned upon creation of a webhook.</param>
        /// <param name="webhook"></param>
        /// <returns></returns>
        public async Task<VoiceResponse<Webhook>> UpdateWebhook(string id, Webhook webhook)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var resource = new Webhooks(new Webhook { Id = id, url = webhook.url, token = webhook.token });
            var result = await restClient.Update(resource);

            return (VoiceResponse<Webhook>)result.Object;
        }

        /// <summary>
        /// This request deletes a webhook. The parameter is the unique ID of the webhook.
        /// If successful, this request will return an HTTP header of 204 No Content and an empty response.
        /// </summary>
        /// <param name="webhookId">The unique ID of a call generated upon creation.</param>
        public void DeleteWebhook(string webhookId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(webhookId, "webhookId");

            var resource = new Webhooks(new Webhook { Id = webhookId });

            restClient.Delete(resource);
        }

        #endregion

        #region Voice Messaging API

        public async Task<VoiceMessage> SendVoiceMessage(string body, long[] msisdns, VoiceMessageOptionalArguments optionalArguments = null)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(body, "body");
            ParameterValidator.ContainsAtLeast(msisdns, 1, "msisdns");

            var recipients = new Recipients(msisdns);
            var voiceMessage = new VoiceMessage(body, recipients, optionalArguments);
            var voiceMessages = new VoiceMessages(voiceMessage);
            var result = await restClient.Create(voiceMessages);

            return result.Object as VoiceMessage;
        }

        public async Task<VoiceMessage> ViewVoiceMessage(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var voiceMessageToView = new VoiceMessages(new VoiceMessage(id));
            var result = await restClient.Retrieve(voiceMessageToView);

            return result.Object as VoiceMessage;
        }

        /// <summary>
        /// This request retrieves a listing of all voice messages from the account.
        /// If successful, this request returns an object with a data property, which is an array that has 0 or more voice message objects.
        /// </summary>
        /// <param name="limit">Set how many records will return from the server</param>
        /// <param name="offset">Identify the starting point to return rows from a result</param>
        /// <returns>If successful, this request will return an object with a data, _links and pagination properties.</returns>
        public async Task<VoiceMessageList> ListVoiceMessages(int limit = 20, int offset = 0)
        {
            var voiceMessageLists = new VoiceMessageLists();
            var voiceMessageList = new VoiceMessageLists(new VoiceMessageList { Limit = limit, Offset = offset });
            
            await restClient.Retrieve(voiceMessageList);
            return voiceMessageList.Object as VoiceMessageList;
        }

        #endregion

        #region Verify API

        public async Task<Objects.Verify> SendVerifyToken(string id, string token)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");
            ParameterValidator.IsNotNullOrWhiteSpace(token, "token");

            var verify = new Objects.Verify(id, token);
            var verifyResource = new Resources.Verify(verify);
            var result = await restClient.Retrieve(verifyResource);

            return result.Object as Objects.Verify;
        }

        // Alias for the old constructor so that it remains backwards compatible
        public async Task<Objects.Verify> CreateVerify(string recipient, VerifyOptionalArguments arguments = null)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(recipient, "recipient");

            return await CreateVerify(Convert.ToInt64(recipient), arguments);
        }

        public async Task<Objects.Verify> CreateVerify(long recipient, VerifyOptionalArguments arguments = null)
        {
            var verify = new Objects.Verify(recipient, arguments);
            var verifyResource = new Resources.Verify(verify);
            var result = await restClient.Create(verifyResource);

            return result.Object as Objects.Verify;
        }

        public void DeleteVerify(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var verify = new Objects.Verify(id);
            var verifyResource = new Resources.Verify(verify);

            restClient.Delete(verifyResource);
        }

        public async Task<Objects.Verify> ViewVerify(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var verify = new Objects.Verify(id);
            var verifyResource = new Resources.Verify(verify);
            var result = await restClient.Retrieve(verifyResource);

            return result.Object as Objects.Verify;
        }

        #endregion

        #region HLR API

        public async Task<Objects.Hlr> RequestHlr(long msisdn, string reference)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(reference, "reference");

            var hlrToRequest = new Resources.Hlr(new Objects.Hlr(msisdn, reference));
            var result = await restClient.Create(hlrToRequest);

            return result.Object as Objects.Hlr;
        }

        public async Task<Objects.Hlr> ViewHlr(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var hlrToView = new Resources.Hlr(new Objects.Hlr(id));
            var result = await restClient.Retrieve(hlrToView);

            return result.Object as Objects.Hlr;
        }

        #endregion

        #region Balance API

        public async Task<Objects.Balance> Balance()
        {
            var balance = new Resources.Balance();
            var result = await restClient.Retrieve(balance);

            return result.Object as Objects.Balance;
        }

        #endregion

        #region Lookup API

        public async Task<Objects.Lookup> ViewLookup(long phonenumber, LookupOptionalArguments optionalArguments = null)
        {
            var lookup = new Resources.Lookup(new Objects.Lookup(phonenumber, optionalArguments));
            var result = await restClient.Retrieve(lookup);

            return result.Object as Objects.Lookup;
        }

        public async Task<Objects.LookupHlr> RequestLookupHlr(long phonenumber, string reference, LookupHlrOptionalArguments optionalArguments = null)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(reference, "reference");

            var lookupHlr = new Resources.LookupHlr(new Objects.LookupHlr(phonenumber, reference, optionalArguments));
            var result =await restClient.Create(lookupHlr);

            return result.Object as Objects.LookupHlr;
        }

        public async Task<Objects.LookupHlr> ViewLookupHlr(long phonenumber, LookupHlrOptionalArguments optionalArguments = null)
        {
            var lookupHlr = new Resources.LookupHlr(new Objects.LookupHlr(phonenumber, optionalArguments));
            var result = await restClient.Retrieve(lookupHlr);

            return result.Object as Objects.LookupHlr;
        }

        #endregion

        #region Contacts API

        public async Task<Contact> CreateContact(long msisdn, ContactOptionalArguments optionalArguments = null)
        {
            var contact = new Contact { Msisdn = msisdn };
            if (optionalArguments != null)
            {
                contact.FirstName = optionalArguments.FirstName;
                contact.LastName = optionalArguments.LastName;
                contact.CustomDetails = new ContactCustomDetails
                {
                    Custom1 = optionalArguments.Custom1,
                    Custom2 = optionalArguments.Custom2,
                    Custom3 = optionalArguments.Custom3,
                    Custom4 = optionalArguments.Custom4,
                };
            }

            var result = await restClient.Create(new Contacts(contact));

            return result.Object as Contact;
        }

        public void DeleteContact(string id)
        {
            restClient.Delete(new Contacts(new Contact { Id = id }));
        }

        public async Task<ContactList> ListContacts(int limit = 20, int offset = 0)
        {
            var contactLists = new ContactLists();

            var contactList = (ContactList)contactLists.Object;
            contactList.Limit = limit;
            contactList.Offset = offset;

            await restClient.Retrieve(contactLists);

            return contactLists.Object as ContactList;
        }

        public async Task<Contact> ViewContact(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var contacts = new Contacts(new Contact { Id = id });
            await restClient.Retrieve(contacts);

            return contacts.Object as Contact;
        }

        public Contact UpdateContact(string id, ContactOptionalArguments optionalArguments)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var customDetails = new ContactCustomDetails
            {
                Custom1 = optionalArguments.Custom1,
                Custom2 = optionalArguments.Custom2,
                Custom3 = optionalArguments.Custom3,
                Custom4 = optionalArguments.Custom4,
            };

            var contacts = new Contacts(new Contact
            {
                Id = id,
                FirstName = optionalArguments.FirstName,
                LastName = optionalArguments.LastName,
                CustomDetails = customDetails,
            });

            restClient.Update(contacts);

            return contacts.Object as Contact;
        }

        #endregion

        #region Groups API

        public async Task<Group> CreateGroup(string name)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(name, "name");

            var groups = new Groups(new Group { Name = name });
            var result = await restClient.Create(groups);

            return result.Object as Group;
        }

        public void DeleteGroup(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var groups = new Groups(new Group { Id = id });

            restClient.Delete(groups);
        }

        public async Task<GroupList> ListGroups(int limit = 20, int offset = 0)
        {
            var groupLists = new GroupLists();

            var groupList = (GroupList)groupLists.Object;
            groupList.Limit = limit;
            groupList.Offset = offset;

            await restClient.Retrieve(groupLists);

            return groupLists.Object as GroupList;
        }

        public Group UpdateGroup(string id, string name)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var groups = new Groups(new Group
            {
                Id = id,
                Name = name,
            });

            restClient.Update(groups);

            return groups.Object as Group;
        }

        public async Task<Group> ViewGroup(string id)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(id, "id");

            var groups = new Groups(new Group { Id = id });
            await restClient.Retrieve(groups);

            return groups.Object as Group;
        }

        public void AddContactsToGroup(string groupId, IEnumerable<string> contactIds)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(groupId, "groupId");

            var uri = string.Format("groups/{0}?{1}", groupId, GetAddContactsToGroupQuery(contactIds));

            restClient.PerformHttpRequest("GET", uri, HttpStatusCode.NoContent, baseUrl: Resource.DefaultBaseUrl);
        }

        /// <summary>
        /// Gets a query string to add contact IDs to a group. This uses a
        /// specific format: ids[]=foo&ids[]=bar. Given the structure of
        /// RestClient, this is easier done by providing the HTTP method as URL
        /// parameter. See:
        /// https://developers.messagebird.com/docs/groups#add-contact-to-group
        /// https://developers.messagebird.com/docs/groups#add-contact-to-group
        /// </summary>
        private string GetAddContactsToGroupQuery(IEnumerable<string> contactIds)
        {
            var parameters = new List<string>();
            parameters.Add("_method=PUT");

            foreach (var contactId in contactIds)
            {
                parameters.Add("ids[]=" + contactId);
            }

            return string.Join("&", parameters);
        }

        public void RemoveContactFromGroup(string groupId, string contactId)
        {
            ParameterValidator.IsNotNullOrWhiteSpace(groupId, "groupId");
            ParameterValidator.IsNotNullOrWhiteSpace(contactId, "contactId");

            var uri = string.Format("groups/{0}/contacts/{1}", groupId, contactId);

            restClient.PerformHttpRequest("DELETE", uri, HttpStatusCode.NoContent, baseUrl: Resource.DefaultBaseUrl);
        }

        #endregion
    }
}
