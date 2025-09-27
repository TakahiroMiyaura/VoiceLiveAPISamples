// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message
{

    /// <summary>  
    /// Represents an error message in the VoiceLiveAPI.  
    /// </summary>  
    public class Error : MessageBase
    {
        #region Static Fields and Constants

        /// <summary>  
        /// The type of the error message.  
        /// </summary>  
        public const string Type = "error";

        #endregion

        #region Public Fields

        /// <summary>  
        /// Gets or sets the details of the error.  
        /// </summary>  
        public ErrorDetail error { get; set; }

        #endregion

        #region Constructors

        /// <summary>  
        /// Initializes a new instance of the <see cref="Error"/> class.  
        /// </summary>  
        public Error()
        {
            type = Type;
            error = new ErrorDetail();
        }

        #endregion
    }
}
