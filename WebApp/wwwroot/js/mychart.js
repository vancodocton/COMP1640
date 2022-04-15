const config = {
    type: 'bar',
    data: {
        labels: ['Academic', 'Support'],
        datasets: [
            {
                label: 'React',
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
                data: [30, 20, 40],
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
                data: [30, 20, 40],
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
};
var ctx = document.getElementById("myChart").getContext('2d');
ctx.width = 100;
ctx.height = 100;
var my = new Chart(ctx,
    config
);