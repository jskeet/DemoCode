// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections;
using System.Collections.Generic;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// The result of a validation failure when loading data.
    /// </summary>
    public sealed class DataValidationError
    {
        public static IEnumerable<DataValidationError> None { get; } = new EmptyErrorCollection();
        
        public IDataField Field { get; }
        public string Message { get; }

        /// <summary>
        /// If <see cref="Field"/> refers to a field within an overlay,
        /// this is the containing field.
        /// </summary>
        public IDataField? OverlayParentField { get; }
        /// <summary>
        /// If <see cref="Field"/> refers to a field within an overlay,
        /// this is the description of the overlay being applied.
        /// </summary>
        public string? OverlayDescription { get; }

        internal DataValidationError(IDataField field, string message) =>
            (Field, Message) = (field, message);

        internal DataValidationError(IDataField field, string message, IDataField overlayParentField, string overlayDescription)
            : this(field, message) =>
            (OverlayParentField, OverlayDescription) = (overlayParentField, overlayDescription);

        /// <summary>
        /// The path to the field, including any invalid
        /// </summary>
        public string Path => OverlayParentField is null
            ? Field.SchemaField.Path
            : $"{OverlayParentField.SchemaField.Path}/{{{OverlayDescription}}}{Field.SchemaField.Path}";

        public override string ToString() => $"{Message} (path: {Path})";

        /// <summary>
        /// Implementation of both IEnumerable and IEnumerator for efficient handling without allocation.
        /// </summary>
        private class EmptyErrorCollection : IEnumerable<DataValidationError>, IEnumerator<DataValidationError>
        {
            public DataValidationError Current => throw new System.NotImplementedException();

            object IEnumerator.Current => throw new System.NotImplementedException();

            public void Dispose() { }

            public IEnumerator<DataValidationError> GetEnumerator() => this;

            public bool MoveNext() => false;

            public void Reset()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator() => this;
        }
    }
}
