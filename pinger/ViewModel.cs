using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows;

namespace pinger
{
    public class MyViewModel
    {
        public ObservableDataSource<Point> Data { get; set; }
        public MyViewModel() { Data = new ObservableDataSource<Point>(); }
    }
}
