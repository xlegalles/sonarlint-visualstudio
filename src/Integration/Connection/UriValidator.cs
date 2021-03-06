//-----------------------------------------------------------------------
// <copyright file="UriValidator.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SonarLint.VisualStudio.Integration.Connection
{
    internal class UriValidator
    {
        private static readonly ISet<string> DefaultSupportedSchemes = new HashSet<string>(new[] { "http", "https" }, StringComparer.OrdinalIgnoreCase);

        private static readonly ISet<string> DefaultInsecureSchemes = new HashSet<string>(new[] { "http" }, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Supported URI schemes.
        /// </summary>
        private readonly ISet<string> supportedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Schemes which are considered to be insecure.
        /// </summary>
        private readonly ISet<string> insecureSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public UriValidator()
        {
            this.supportedSchemes.UnionWith(DefaultSupportedSchemes);
            this.insecureSchemes.UnionWith(DefaultInsecureSchemes);
        }

        public UriValidator(ISet<string> supportedSchemes)
        {
            if (supportedSchemes == null)
            {
                throw new ArgumentNullException(nameof(supportedSchemes));
            }

            this.supportedSchemes.Clear();
            this.insecureSchemes.Clear();

            this.supportedSchemes.UnionWith(supportedSchemes);
        }

        public UriValidator(ISet<string> supportedSchemes, ISet<string> insecureSchemes)
            : this(supportedSchemes)
        {
            if (insecureSchemes == null)
            {
                throw new ArgumentNullException(nameof(supportedSchemes));
            }

            this.supportedSchemes.Clear();
            this.insecureSchemes.Clear();

            this.supportedSchemes.UnionWith(supportedSchemes);
            this.insecureSchemes.UnionWith(insecureSchemes);

            if (!this.insecureSchemes.IsSubsetOf(this.supportedSchemes))
            {
                throw new ArgumentException(Resources.Strings.ExceptionInsecureSchemesIsNotSubset, nameof(insecureSchemes));
            }
        }

        /// <summary>
        /// True if <paramref name="uri"/>'s scheme is one of <see cref="supportedSchemes"/>, false otherwise.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="uri"><see cref="Uri"/> to check, must not be null.</param>
        public bool IsSupportedScheme(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return this.supportedSchemes?.Contains(uri.Scheme) ?? false;
        }

        /// <summary>
        /// True if <paramref name="uri"/>'s scheme is one of <see cref="insecureSchemes"/>, false otherwise.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="uri"><see cref="Uri"/> to check, must not be null.</param>
        public bool IsInsecureScheme(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return this.insecureSchemes?.Contains(uri.Scheme) ?? false;
        }

        /// <summary>
        /// Whether or not <paramref name="uriString"/> is considered to be a valid URI.
        /// </summary>
        /// <param name="uriString"><see cref="string"/> URI to check</param>
        /// <remarks>
        /// Valid URIs cannot be null, must have a scheme listed in <see cref="supportedSchemes"/>,
        /// and be absolute.
        /// </remarks>
        public virtual bool IsValidUri(string uriString)
        {
            // non empty
            if (string.IsNullOrWhiteSpace(uriString))
            {
                return false;
            }

            Uri uri;

            // creatable
            UriKind kind = UriKind.Absolute;
            if (!Uri.TryCreate(uriString, kind, out uri))
            {
                return false;
            }

            return this.IsValidUri(uri);
        }

        /// <summary>
        /// Whether or not <paramref name="uri"/> is considered to be a valid URI.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="uri"><see cref="Uri"/> to check, must not be null.</param>
        /// <remarks>
        /// Valid URIs must have a scheme listed in <see cref="supportedSchemes"/>
        /// and be absolute.
        /// </remarks>
        public virtual bool IsValidUri(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            // absolute
            if (!uri.IsAbsoluteUri)
            {
                return false;
            }

            // supported
            if (!this.IsSupportedScheme(uri))
            {
                return false;
            }

            return true;
        }
    }
}
