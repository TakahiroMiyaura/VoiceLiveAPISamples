// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Generic;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents a set of parameters with their types, descriptions, and required fields.  
    /// </summary>  
    public class Params
    {
        /// <summary>  
        /// Gets or sets the type of the parameter object. Default is "object".  
        /// </summary>  
        public string type { get; set; } = "object";

        /// <summary>  
        /// Gets or sets the dictionary of parameters, where the key is the parameter name and the value is the parameter details.  
        /// </summary>  
        public Dictionary<string, Param> parameters { get; set; } = null;

        /// <summary>  
        /// Gets or sets the list of required parameter names.  
        /// </summary>  
        public string[] required { get; set; } = null;
    }
}