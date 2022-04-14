var datapie = [{
    data: [30, 55],
    backgroundColor: [
        "#5f255f",
        "#B27200"
    ],
    borderColor: "#fff"
}];

var options = {
    responsive: true,
    maintainAspectRatio: false,
    tooltips: {
        enabled: false
    },
    plugins: {
        title: {
            display: true,
            text: 'Percentage of ideas by each Department.'
        },
        datalabels: {
            formatter: (value, ctx) => {
                const datapoints = ctx.chart.data.datasets[0].data
                const total = datapoints.reduce((total, datapoint) => total + datapoint, 0)
                const percentage = value / total * 100
                return percentage.toFixed(2) + "%";
            },
            color: '#fff',
        }
    }
};

var ctx = document.getElementById("pieChart").getContext('2d');
ctx.width = 50;
ctx.height = 50;
var pieChart = new Chart(ctx, {
    type: 'pie',
    data: {
        labels: ['Academic Department', 'Support Department'],
        datasets: datapie
    },
    options: options,
    plugins: [ChartDataLabels],
});