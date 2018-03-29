//-----------------------------------------------------------------------
// <copyright file="SerializationOptions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Sirenix.Serialization
{
    /// <summary>
    /// Defines the configuration during serialization and deserialization. This class is thread-safe.
    /// </summary>
    public class SerializationConfig
    {
        private readonly object LOCK = new object();
        private volatile ISerializationPolicy serializationPolicy;
        private volatile DebugContext debugContext;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public SerializationConfig()
        {
            this.ResetToDefault();
        }

        /// <summary>
        /// Gets or sets the serialization policy. This value is never null; if set to null, it will default to <see cref="SerializationPolicies.Unity"/>.
        /// </summary>
        /// <value>
        /// The serialization policy.
        /// </value>
        public ISerializationPolicy SerializationPolicy
        {
            get
            {
                if (this.serializationPolicy == null)
                {
                    lock (this.LOCK)
                    {
                        if (this.serializationPolicy == null)
                        {
                            this.serializationPolicy = SerializationPolicies.Unity;
                        }
                    }
                }

                return this.serializationPolicy;
            }

            set
            {
                lock (this.LOCK)
                {
                    this.serializationPolicy = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the debug context. This value is never null; if set to null, a new default instance of <see cref="Sirenix.Serialization.DebugContext"/> will be created upon the next get.
        /// </summary>
        /// <value>
        /// The debug context.
        /// </value>
        public DebugContext DebugContext
        {
            get
            {
                if (this.debugContext == null)
                {
                    lock (this.LOCK)
                    {
                        if (this.debugContext == null)
                        {
                            this.debugContext = new DebugContext();
                        }
                    }
                }

                return this.debugContext;
            }

            set
            {
                lock (this.LOCK)
                {
                    this.debugContext = value;
                }
            }
        }

        /// <summary>
        /// Resets the configuration to a default configuration, as if the constructor had just been called.
        /// </summary>
        public void ResetToDefault()
        {
            lock (this.LOCK)
            {
                this.serializationPolicy = null;
                this.debugContext = null;
            }
        }
    }

    /// <summary>
    /// Defines a context for debugging and logging during serialization and deserialization. This class is thread-safe.
    /// </summary>
    public sealed class DebugContext
    {
        private readonly object LOCK = new object();

        private volatile ILogger logger;
        private volatile LoggingPolicy loggingPolicy;
        private volatile ErrorHandlingPolicy errorHandlingPolicy;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                if (this.logger == null)
                {
                    lock (this.LOCK)
                    {
                        if (this.logger == null)
                        {
                            this.logger = DefaultLoggers.UnityLogger;
                        }
                    }
                }

                return this.logger;
            }
            set
            {
                lock (this.LOCK)
                {
                    this.logger = value;
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public LoggingPolicy LoggingPolicy
        {
            get { return this.loggingPolicy; }
            set { this.loggingPolicy = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public ErrorHandlingPolicy ErrorHandlingPolicy
        {
            get { return this.errorHandlingPolicy; }
            set { this.errorHandlingPolicy = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void LogWarning(string message)
        {
            if (this.errorHandlingPolicy == ErrorHandlingPolicy.ThrowOnWarningsAndErrors)
            {
                throw new SerializationAbortException("The following warning was logged during serialization or deserialization: " + (message ?? "EMPTY EXCEPTION MESSAGE"));
            }

            if (this.loggingPolicy == LoggingPolicy.LogWarningsAndErrors)
            {
                this.Logger.LogWarning(message);
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void LogError(string message)
        {
            if (this.errorHandlingPolicy != ErrorHandlingPolicy.Resilient)
            {
                throw new SerializationAbortException("The following error was logged during serialization or deserialization: " + (message ?? "EMPTY EXCEPTION MESSAGE"));
            }

            if (this.loggingPolicy != LoggingPolicy.Silent)
            {
                this.Logger.LogError(message);
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void LogException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            // We must always rethrow abort exceptions
            if (exception is SerializationAbortException)
            {
                throw exception;
            }

            var policy = this.errorHandlingPolicy;

            if (policy != ErrorHandlingPolicy.Resilient)
            {
                throw new SerializationAbortException("An exception of type " + exception.GetType().Name + " occurred during serialization or deserialization.", exception);
            }

            if (this.loggingPolicy != LoggingPolicy.Silent)
            {
                this.Logger.LogException(exception);
            }
        }
    }
}