using System;
using System.Runtime.Serialization;

namespace AndroidLib.Unity
{
  /// Represents an exception that occurred when interacting with the aDevice API.
  [Serializable]
  public class aDeviceException : Exception
  {
    /// <summary>
    /// Backing field for the <see cref="P:AndroidConnLib.androidConnectionManager.aDeviceException.ErrorCode" /> property.
    /// </summary>
    private aDeviceError errorCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:AndroidConnLib.androidConnectionManager.aDeviceException" /> class.
    /// </summary>
    public aDeviceException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:AndroidConnLib.androidConnectionManager.aDeviceException" /> class with a specified error code.
    /// </summary>
    /// <param name="error">The error code of the error that occurred.</param>
    public aDeviceException(aDeviceError error)
      : base(string.Format("An aDevice error occurred. The error code was {0}", (object)error))
    {
      this.errorCode = error;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:AndroidConnLib.androidConnectionManager.aDeviceException" /> class with a specified error code and error message.
    /// </summary>
    /// <param name="error">The error code of the error that occurred.</param>
    /// <param name="message">A message which describes the error.</param>
    public aDeviceException(aDeviceError error, string message)
      : base(string.Format("An aDevice error occurred. {1}. The error code was {0}", (object)error, (object)message))
    {
      this.errorCode = error;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:AndroidConnLib.androidConnectionManager.aDeviceException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public aDeviceException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:AndroidConnLib.androidConnectionManager.aDeviceException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// </param>
    /// <param name="inner">
    /// The exception that is the cause of the current exception, or <see langword="null" /> if no inner exception is specified.
    /// </param>
    public aDeviceException(string message, Exception inner)
      : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:AndroidConnLib.androidConnectionManager.aDeviceException" /> class with serialized data.
    /// </summary>
    /// <param name="info">
    /// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
    /// </param>
    protected aDeviceException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    /// <summary>Gets the error code that represents the error.</summary>
    public virtual aDeviceError ErrorCode => this.errorCode;
  }
}