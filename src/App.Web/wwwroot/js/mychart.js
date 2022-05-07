const config = {
    type: 'bar',
    data: {
        labels: datasets.map(item => item.label),
        datasets: [
            {
                label: 'Idea',
                data: datasets,
                parsing: {
                    xAxisKey: 'label',
                    yAxisKey: 'countIdea'
                },
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)'
                ],
                borderWidth: 1
            },
            {
                label: 'Comment',
                data: datasets,
                parsing: {
                    xAxisKey: 'label',
                    yAxisKey: 'countComment'
                },
                backgroundColor: [
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 159, 64)'
                ],
                borderWidth: 1
            },
            {
                label: 'React',
                data: datasets,
                parsing: {
                    xAxisKey: 'label',
                    yAxisKey: 'countReact'
                },
                backgroundColor: [
                    'rgba(75, 192, 192, 0.2)',
                ],
                borderColor: [
                    'rgb(75, 192, 192)'
                ],
                borderWidth: 1
            }
        ]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: 'Contribution of each Department'
            }
        }
    },
};
var ctx = document.getElementById("barChart").getContext('2d');
ctx.width = 100;
ctx.height = 100;
var barChart = new Chart(ctx,
    config
);