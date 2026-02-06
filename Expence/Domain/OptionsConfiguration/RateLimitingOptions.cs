namespace Expence.Domain.OptionsConfiguration
{
        /// <summary>
        /// Configuration options for rate limiting policies
        /// </summary>
        public class RateLimitingOptions
        {
            public const string SectionName = "RateLimiting";

            /// <summary>
            /// Global rate limiting policy configuration
            /// </summary>
            public GlobalPolicyOptions Global { get; set; } = new();

            /// <summary>
            /// Authentication endpoints rate limiting (stricter)
            /// </summary>
            public AuthPolicyOptions Auth { get; set; } = new();

            /// <summary>
            /// Transaction endpoints rate limiting
            /// </summary>
            public TransactionPolicyOptions Transaction { get; set; } = new();

            /// <summary>
            /// AI features endpoints rate limiting (more expensive operations)
            /// </summary>
            public AiPolicyOptions AiFeatures { get; set; } = new();

            public class GlobalPolicyOptions
            {
                /// <summary>
                /// Number of requests allowed per time window
                /// </summary>
                public int PermitLimit { get; set; } = 100;

                /// <summary>
                /// Time window in seconds
                /// </summary>
                public int WindowSizeInSeconds { get; set; } = 60;

                /// <summary>
                /// Queue size for requests waiting to be processed
                /// </summary>
                public int QueueLimit { get; set; } = 2;
            }

            public class AuthPolicyOptions
            {
                /// <summary>
                /// Number of login/register attempts allowed
                /// </summary>
                public int PermitLimit { get; set; } = 5;

                /// <summary>
                /// Time window in seconds (15 minutes for auth)
                /// </summary>
                public int WindowSizeInSeconds { get; set; } = 900;

                /// <summary>
                /// Queue limit for queued requests
                /// </summary>
                public int QueueLimit { get; set; } = 1;
            }

            public class TransactionPolicyOptions
            {
                /// <summary>
                /// Number of transaction requests allowed
                /// </summary>
                public int PermitLimit { get; set; } = 50;

                /// <summary>
                /// Time window in seconds
                /// </summary>
                public int WindowSizeInSeconds { get; set; } = 60;

                /// <summary>
                /// Queue limit
                /// </summary>
                public int QueueLimit { get; set; } = 2;
            }

            public class AiPolicyOptions
            {
                /// <summary>
                /// Number of AI requests allowed (more restrictive due to cost)
                /// </summary>
                public int PermitLimit { get; set; } = 10;

                /// <summary>
                /// Time window in seconds (5 minutes for AI)
                /// </summary>
                public int WindowSizeInSeconds { get; set; } = 300;

                /// <summary>
                /// Queue limit
                /// </summary>
                public int QueueLimit { get; set; } = 1;
            }
        }
    
}
