using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab4
{
    public partial class ConwayMain : Form
    {
        Boolean InProgress;
        Grid CellGrid;

        public ConwayMain()
        {
            InitializeComponent();
        }

        private void ConwayMain_Load(object sender, EventArgs e)
        {
            CreateGridSurface();
            //GetActiveCounts();
        }

        private void CreateGridSurface()
        {
            Cell newCell;
            Random random = new Random();

            //Determine number of rows and columns in grid
            int rows = (int)(pbGrid.Height / numSSize.Value);
            int cols = (int)(pbGrid.Width / numSSize.Value);

            //Create grid object
            CellGrid = new Grid(rows, cols);

            //Clear and rebuild the list of cells, activating 15% of cells at random
            Grid.gridCells.Clear();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    newCell = new Cell(x, y, (int)numSSize.Value);
                    newCell.IsAlive = (random.Next(100) < 15) ? true : false;
                }
            }

            Grid.gridCells = Grid.gridCells.OrderBy(c => c.XPos).OrderBy(c => c.YPos).ToList();

            UpdateGrid(CellGrid);
        }

        private void GetActiveCounts()
        {
            cboCells.Items.Clear();

            foreach (Cell Cell in Grid.gridCells)
            {
                cboCells.Items.Add($"X:{Cell.XPos} ,Y:{Cell.YPos} , Count:{CellGrid.LiveAdjacent(Cell)} ");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            CreateGridSurface();
            //GetActiveCounts();
        }

        private void GetNextState()
        {
            //Calculate the next status of each cell
            foreach (Cell cell in Grid.gridCells)
            {
                int activeCount = CellGrid.LiveAdjacent(cell);

                if (cell.IsAlive)
                {
                    if ((activeCount < 2) || (activeCount > 3))
                        cell.NextStatus = false;
                    else
                        cell.NextStatus = true;
                }
                else
                {
                    if (activeCount == 3)
                        cell.NextStatus = true;
                }
            }

            //for each cell, update IsAlive with NextStatus
            foreach (Cell cell in Grid.gridCells)
            {
                cell.IsAlive = cell.NextStatus;
            }

            UpdateGrid(CellGrid);
        }

        private void UpdateGrid (Grid GridDisplay)
        {
            Cell newCell;
            Random random = new Random();

            //Create new image and update new picture box
            using (Bitmap bmp = new Bitmap(pbGrid.Width, pbGrid.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush cellBrush = new SolidBrush(Color.DarkOrange))
            {
                g.Clear(Color.Black);

                foreach (Cell cell in Grid.gridCells)
                {
                    if (cell.IsAlive)
                    {
                        g.FillRectangle(cellBrush, cell.CellDisplay);
                    }
                }

                if (pbGrid.Image!= null)
                    pbGrid.Image.Dispose();

                pbGrid.Image = (Bitmap)bmp.Clone();
            }
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            GetNextState();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            //Flip the in progress variable
            InProgress = !InProgress;
            
            //Change the text in the button
            btnGo.Text = InProgress ? "Stop" : "Go";

            //While inProgress = true, continue to update grid
            while (InProgress)
            {
                GetNextState();
                Application.DoEvents();
            }
        }

        private void ConwayMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            InProgress = false;
            Application.Exit();
        }
    }

    public class Grid
    {
        public static List<Cell> gridCells = new List<Cell>();

        private int cRows;
        private int cCols;

        public Grid (int rows, int columns)
        {
            this.Rows = rows;
            this.Columns = columns;
        }

        public int Rows
        {
            get { return cRows; }
            set { cRows = value; }
        }

        public int Columns
        {
            get { return cCols; }
            set { cCols = value; }
        }

        public int LiveAdjacent(Cell cell)
        {
            //Function to return number of active neighboring cells
            //Use cell index numbers to search range of cells for active neighbors
            //Sort cells grid collection by X and Y position to ensure accurate range
            //Use for loop to search a range of cells within the collective including the rows immediately before and after the center cell rather than the entire grid

            int liveAdjacent = 0;

            //Get range of cells to be examined for active neighbors 
            int cellIndex = (cell.YPos * this.Columns) + cell.XPos;
            int startIndex = cellIndex - this.Columns - 2;
            int endIndex = cellIndex + this.Columns + 2;

            //Ensure that the start and end indexes don't exceed the grid range
            startIndex = (startIndex < 0) ? 0 : startIndex;
            endIndex = (endIndex > (Grid.gridCells.Count - 1)) ? Grid.gridCells.Count - 1 : endIndex;

            //Iterate through the defined range and look for active neighbors
            for (int x = startIndex; x < endIndex; x++)
            {
                if (Math.Abs(cell.XPos - gridCells[x].XPos) < 2 && Math.Abs(cell.YPos - gridCells[x].YPos) < 2)
                {
                    if (Grid.gridCells[x].Location != cell.Location)
                    {
                        if (gridCells[x].IsAlive)
                        {
                            liveAdjacent++;
                        }
                    }
                }
            }

            return liveAdjacent;
        }
    }

    public class Cell
    {
        private Point clocation;
        private Rectangle cCellDisplay;
        private Size cCellSize;
        private int cXPos;
        private int cYPos;
        private Boolean cIsAlive;
        private Boolean cNext;

        public Cell(Point location, int X, int Y)
        {
            int cellSize;
            //Set object settings and add to grid
            this.Location = location;
            this.YPos = Y;
            this.XPos = X;
            Grid.gridCells.Add(this);
            //If location is not 0, divide pixel location by grid location to get size of the cell
            cellSize = (X == 0) ? 0 : location.X / X;

            // Create rectangle to display as needed using calculated cell size
            this.cCellDisplay = new Rectangle(this.Location, new Size(cellSize, cellSize));
        }

        public Cell(int X, int Y, int CellSize)
        {
            this.Location = new Point(X * CellSize, Y * CellSize);
            this.XPos= X;
            this.YPos= Y;
            this.CellSize = new Size(CellSize, CellSize);
            Grid.gridCells.Add(this);
            this.CellDisplay = new Rectangle(this.Location, new Size(CellSize - 1, CellSize - 1));
        }

        public Rectangle CellDisplay
        {
            //Rectangle object for displaying cell when needed
            get { return cCellDisplay; }
            set { cCellDisplay = value; }
        }

        public Size CellSize
        {
            //Calcualte cell size
            get { return cCellSize; }
            set { cCellSize = value; }
        }

        public Point Location
        {
            //X, Y Point that specifies cell location
            get { return clocation; }
            set { clocation = value; }
        }

        public int XPos
        {
            get { return cXPos; }
            set { cXPos = value; }
        }

        public int YPos
        {
            get { return cYPos; }
            set { cYPos = value; }
        }

        public Boolean IsAlive
        {
            get { return cIsAlive; }
            set { cIsAlive = value; }
        }

        public Boolean NextStatus
        {
            get { return cNext; }
            set { cNext = value; }
        }

        public override string ToString()
        {
            //Override the cell ToString to provide location information
            return $"GridX:{this.XPos} GridY:{this.YPos} LocX:{this.Location.X} LocY:{this.Location.Y}";
        }
    }
}
