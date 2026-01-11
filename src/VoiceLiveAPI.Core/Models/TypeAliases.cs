// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

// This file provides migration guidance from legacy message types to the new unified Models.
// The new Models in Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models provide:
// - Shorter, more intuitive class names
// - Consistent naming conventions
// - Built-in convenience properties
// - Factory pattern support via VoiceLiveModelFactory

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Compatibility
{
    // ================================================================================================
    // MIGRATION GUIDE: Server Messages (Verified)
    // ================================================================================================
    //
    // Old Location: Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
    // New Location: Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
    //
    // | Legacy Class Name                              | New Class Name      | Notes                    |
    // |------------------------------------------------|---------------------|--------------------------|
    // | ResponseAudioDelta                             | AudioDelta          | Built-in Base64 decode   |
    // | ResponseAudioDone                              | AudioDone           |                          |
    // | ResponseAudioTranscriptDelta                   | TranscriptDelta     |                          |
    // | ResponseAudioTranscriptDone                    | TranscriptDone      |                          |
    // | ResponseCreated                                | ResponseCreated     |                          |
    // | ResponseDone                                   | ResponseInfo        | Status helpers added     |
    // | ResponseOutputItemAdded                        | OutputItemAdded     |                          |
    // | ResponseOutputItemDone                         | OutputItemDone      |                          |
    // | ResponseContentPartAdded                       | ContentPartAdded    |                          |
    // | ResponseContentPartDone                        | ContentPartDone     |                          |
    // | ResponseAnimationVisemeDelta                   | VisemeDelta         |                          |
    // | ResponseAnimationVisemeDone                    | VisemeDone          |                          |
    // | SessionCreated                                 | SessionInfo         | Use with event context   |
    // | SessionUpdated                                 | SessionInfo         | Use with event context   |
    // | SessionAvatarConnecting                        | AvatarConnecting    |                          |
    // | ConversationItemCreated                        | ItemCreated         |                          |
    // | ConversationItemInputAudioTranscriptionCompleted | TranscriptionResult | Simplified name        |
    // | InputAudioBufferCommitted                      | AudioCommitted      |                          |
    // | InputAudioBufferSpeechStarted                  | SpeechStarted       |                          |
    // | InputAudioBufferSpeechStopped                  | SpeechStopped       |                          |
    // | Error                                          | VoiceLiveError      |                          |
    //
    // ================================================================================================
    // MIGRATION GUIDE: Server Messages (Unverified)
    // ================================================================================================
    //
    // Old Location: Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
    // New Location: Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
    //
    // | Legacy Class Name                                    | New Class Name        |
    // |------------------------------------------------------|-----------------------|
    // | ConversationItemDeletedMessage                       | ItemDeleted           |
    // | ConversationItemTruncatedMessage                     | ItemTruncated         |
    // | ConversationItemRetrievedMessage                     | ItemRetrieved         |
    // | ConversationCreatedMessage                           | ConversationCreated   |
    // | ConversationItemInputAudioTranscriptionFailedMessage | TranscriptionFailed   |
    // | ResponseTextDeltaMessage                             | TextDelta             |
    // | ResponseTextDoneMessage                              | TextDone              |
    // | ResponseFunctionCallArgumentsDeltaMessage            | FunctionCallDelta     |
    // | ResponseFunctionCallArgumentsDoneMessage             | FunctionCallDone      |
    // | RateLimitsUpdatedMessage                             | RateLimitsUpdated     |
    // | InputAudioBufferClearedMessage                       | InputAudioCleared     |
    // | OutputAudioBufferClearedMessage                      | OutputAudioCleared    |
    // | OutputAudioBufferStartedMessage                      | OutputAudioStarted    |
    // | OutputAudioBufferStoppedMessage                      | OutputAudioStopped    |
    //
    // ================================================================================================
    // MIGRATION GUIDE: Client Messages (Commands)
    // ================================================================================================
    //
    // Old Location: Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages
    //              Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Unverified.Messages
    // New Location: Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
    //
    // | Legacy Class Name                 | New Class Name    | Notes                    |
    // |-----------------------------------|-------------------|--------------------------|
    // | InputAudioBufferAppend            | AudioInput        | FromBytes() helper       |
    // | InputAudioBufferCommitMessage     | AudioCommit       |                          |
    // | InputAudioBufferClearMessage      | AudioClear        |                          |
    // | OutputAudioBufferClearMessage     | OutputAudioClear  |                          |
    // | ResponseCreateMessage             | ResponseCreate    |                          |
    // | ResponseCancelMessage             | ResponseCancel    |                          |
    // | ConversationItemCreateMessage     | ItemCreate        |                          |
    // | ConversationItemDeleteMessage     | ItemDelete        |                          |
    // | ConversationItemTruncateMessage   | ItemTruncate      |                          |
    // | ConversationItemRetrieveMessage   | ItemRetrieve      |                          |
    // | ClientSessionUpdated              | SessionUpdate     |                          |
    // | SessionAvatarConnect              | AvatarConnect     |                          |
    //
    // ================================================================================================
    // USAGE EXAMPLES
    // ================================================================================================
    //
    // Before (Legacy):
    // ----------------
    //     using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message;
    //
    //     void HandleAudio(ResponseAudioDelta message)
    //     {
    //         var audio = Convert.FromBase64String(message.Delta);
    //         PlayAudio(audio);
    //     }
    //
    // After (New Models):
    // -------------------
    //     using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
    //
    //     void HandleAudio(AudioDelta message)
    //     {
    //         var audio = message.AudioData;  // Already decoded!
    //         PlayAudio(audio.ToArray());
    //     }
    //
    // With SessionUpdate pattern (Recommended):
    // -----------------------------------------
    //     using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates;
    //
    //     await foreach (var update in session.GetUpdatesAsync())
    //     {
    //         switch (update)
    //         {
    //             case SessionUpdateResponseAudioDelta audio:
    //                 PlayAudio(audio.AudioData.ToArray());
    //                 break;
    //             case SessionUpdateError error:
    //                 Console.WriteLine($"Error: {error.Message}");
    //                 break;
    //             case SessionUpdateInputAudioBufferSpeechStarted:
    //                 Console.WriteLine("User started speaking");
    //                 break;
    //         }
    //     }
    //
    // Using VoiceLiveModelFactory:
    // ----------------------------
    //     using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
    //
    //     // Create for testing
    //     var testAudio = VoiceLiveModelFactory.AudioDelta(
    //         eventId: "evt_123",
    //         responseId: "resp_456",
    //         delta: "base64AudioData...");
    //
    //     // Create from JSON
    //     var audioFromJson = VoiceLiveModelFactory.AudioDeltaFromJson(jsonElement);
    //
    // ================================================================================================
}