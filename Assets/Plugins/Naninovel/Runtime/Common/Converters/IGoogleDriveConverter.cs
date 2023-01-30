// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Implementation is able to convert exported google drive files to <typeparamref name="TResult"/>.
    /// </summary>
    public interface IGoogleDriveConverter<TResult> : IRawConverter<TResult>
    {
        string ExportMimeType { get; }
    }
}
