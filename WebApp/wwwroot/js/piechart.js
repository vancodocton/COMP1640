var configContribution = {
    type: 'pie',
    data: {
        labels: datasets.map(item => item.label),
        datasets: [{
            data: datasets.map(item => item.countIdea + item.countComment + item.countReact),
            backgroundColor: [
                "#5f255f",
                "#B27200"
            ],
            borderColor: "#fff"
        }]
    },
    plugins: [ChartDataLabels],
    options: {
        parsing: {
            key: 'contribution',
        },
        responsive: true,
        maintainAspectRatio: false,
        tooltips: {
            enabled: false
        },
        plugins: {
            title: {
                display: true,
                text: 'Percentage of contribution by each Department.'
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

    }
}
var ctx = document.getElementById("pieChart").getContext('2d');
ctx.width = 50;
ctx.height = 50;
var pieChart = new Chart(ctx, 
    configContribution
);