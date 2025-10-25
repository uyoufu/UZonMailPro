namespace UZonMailProPlugin.Services.IpWarmUp
{
    /// <summary>
    /// 每日发件量计算器
    /// </summary>
    public class DailySendCountCalculator(DateTime startDate, DateTime endDate, int totalInboxes, List<double[]> chartPoints)
    {
        private static readonly int _minCountForToday = 5;

        public int GetCountForToday()
        {
            var today = DateTime.UtcNow;
            if (today < startDate || today > endDate)
            {
                return 0;
            }
            if (totalInboxes <= _minCountForToday)
                return _minCountForToday;

            var totalDays = (endDate.Date - startDate.Date).TotalDays + 1;
            var daysPassed = (today.Date - startDate.Date).TotalDays + 1;
            // 计算今天的百分比
            var percentToday = daysPassed / totalDays;
            // 若没有图表点，则使用 arctan 计算，x 区间为 [-5,5]
            var percent = chartPoints.Count == 0 ? CalculateArctanPercent(percentToday) : InterpolatePercent(percentToday);

            var countForToday = (int)Math.Round(percent * (totalInboxes - _minCountForToday));
            return Math.Min(countForToday, totalInboxes);
        }

        private double CalculateArctanPercent(double percentX)
        {
            // 将 x 映射到 [-5,5]
            var mappedX = percentX * 10 - 5;
            var arctanValue = Math.Atan(mappedX);
            // 将 arctan 值映射到 [0,1]
            var percent = (arctanValue + Math.PI / 2) / Math.PI;
            return percent;
        }

        private double InterpolatePercent(double percentX)
        {
            // 使用 MathNet 进行 3 次样条插值
            var orderedPoints = chartPoints.OrderBy(p => p[0]).ToList();
            var xValues = orderedPoints.Select(p => p[0]).ToArray();
            var yValues = orderedPoints.Select(p => p[1]).ToArray();

            var xMin = xValues.Min();
            var xMax = xValues.Max();

            var spline = MathNet.Numerics.Interpolation.CubicSpline.InterpolateNatural(xValues, yValues);
            var interpolatedValue = spline.Interpolate((xMax - xMin) * percentX + xMin);

            // 确保返回值在 [0,1] 之间
            var minY = yValues.Min();
            var maxY = yValues.Max();
            var percent = (interpolatedValue - minY) / (maxY - minY);
            return percent;
        }
    }
}
