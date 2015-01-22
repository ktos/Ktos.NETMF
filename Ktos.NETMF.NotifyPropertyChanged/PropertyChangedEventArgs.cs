#region License
/*
 * Ktos.NETMF
 *
 * Copyright (C) Marcin Badurowicz <m at badurowicz dot net> 2015
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. 
 */
#endregion


using Microsoft.SPOT;

// ReSharper disable once CheckNamespace
namespace System.ComponentModel
{
    /// <summary>
    /// Provides data for the PropertyChanged event.
    /// </summary>
    public class PropertyChangedEventArgs : EventArgs
    {
        private readonly string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref='System.ComponentModel.PropertyChangedEventArgs'/> class.
        /// </summary>
        /// <param name="propertyName">Property, which was changed</param>
        public PropertyChangedEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Indicates the name of the property that changed.        
        /// </summary>        
        public virtual string PropertyName
        {
            get
            {
                return propertyName;
            }
        }
    }
}
