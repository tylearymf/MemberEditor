#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
    /// <summary>
    /// A helper class to control paging of n number of elements in various situations.
    /// </summary>
    public class GUIPagingHelper
    {
        private bool isEnabled = true;
        private int elementCount;
        private int currentPage;
        private int startIndex;
        private int endIndex;
        private int pageCount;
        private int numberOfItemsPrPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="GUIPagingHelper"/> class.
        /// </summary>
        public GUIPagingHelper()
        {
            this.numberOfItemsPrPage = 1;
        }

        /// <summary>
        /// Updates all values based on <paramref name="elementCount"/> and <see cref="NumberOfItemsPrPage"/>.
        /// </summary>
        /// <remarks>
        /// Call update right before using <see cref="StartIndex"/> and <see cref="EndIndex"/> in your for loop.
        /// </remarks>
        /// <param name="elementCount">The total number of elements to apply paging for.</param>
        public void Update(int elementCount)
        {
            if (elementCount < 0)
            {
                throw new ArgumentOutOfRangeException("Non-negative number required.");
            }

            this.elementCount = elementCount;

            if (this.isEnabled)
            {
                this.pageCount = Mathf.Max(1, Mathf.CeilToInt(this.elementCount / (float)this.numberOfItemsPrPage));
                this.currentPage = Mathf.Clamp(this.currentPage, 0, this.pageCount - 1);
                this.startIndex = this.currentPage * this.numberOfItemsPrPage;
                this.endIndex = Mathf.Min(this.elementCount, this.startIndex + this.numberOfItemsPrPage);
            }
            else
            {
                this.startIndex = 0;
                this.endIndex = this.elementCount;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.isEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on the frist page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is on frist page; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnFirstPage
        {
            get
            {
                return this.currentPage == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on the last page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is on last page; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnLastPage
        {
            get
            {
                return this.currentPage == this.pageCount - 1;
            }
        }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        /// <value>
        /// The number of items pr page.
        /// </value>
        public int NumberOfItemsPerPage
        {
            get { return this.numberOfItemsPrPage; }
            set { this.numberOfItemsPrPage = Mathf.Max(value, 0); }
        }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>
        /// The current page.
        /// </value>
        public int CurrentPage
        {
            get { return this.currentPage; }
            set { this.currentPage = value; }
        }

        /// <summary>
        /// Gets the start index.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        public int StartIndex
        {
            get { return this.startIndex; }
        }

        /// <summary>
        /// Gets the end index.
        /// </summary>
        /// <value>
        /// The end index.
        /// </value>
        public int EndIndex
        {
            get { return this.endIndex; }
        }

        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        public int PageCount
        {
            get { return this.pageCount; }
        }

        /// <summary>
        /// Gets the total number of elements.
        /// Use <see cref="Update(int)"/> to change the value.
        /// </summary>
        public int ElementCount
        {
            get { return this.elementCount; }
        }
    }
}
#endif