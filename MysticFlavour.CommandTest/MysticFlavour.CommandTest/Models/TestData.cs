namespace MysticFlavour.CommandTest.Models
{
    /// <summary>
    /// The test data.
    /// </summary>
    public class TestData
    {
        #region Fields

        /// <summary>
        /// The id.
        /// </summary>
        private int id;

        /// <summary>
        /// The name.
        /// </summary>
        private string name;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }

        #endregion
    }
}
