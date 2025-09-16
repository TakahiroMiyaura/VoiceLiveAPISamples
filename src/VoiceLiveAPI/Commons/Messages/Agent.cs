// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents an agent with associated metadata.  
    /// </summary>  
    public class Agent
    {
        /// <summary>  
        /// Gets or sets the type of the agent.  
        /// </summary>  
        public string type { get; set; } = null;

        /// <summary>  
        /// Gets or sets the name of the agent.  
        /// </summary>  
        public string name { get; set; } = null;

        /// <summary>  
        /// Gets or sets the description of the agent.  
        /// </summary>  
        public object description { get; set; } = null;

        /// <summary>  
        /// Gets or sets the unique identifier of the agent.  
        /// </summary>  
        public string agent_id { get; set; } = null;

        /// <summary>  
        /// Gets or sets the thread identifier associated with the agent.  
        /// </summary>  
        public string thread_id { get; set; } = null;
    }
}