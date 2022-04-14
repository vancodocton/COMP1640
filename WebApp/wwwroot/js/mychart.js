const labels = [
    'Academic Department',
    'Support Department'];
const data = {
    labels: labels,
    datasets: [
        {
            label: 'Idea',
            data: [10, 20, 30],
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
            data: [30, 20, 40],
            backgroundColor: [
                'rgba(75, 192, 192, 0.2)',
            ],
            borderColor: [
                'rgb(75, 192, 192)'
            ],
            borderWidth: 1
        }]
};
const config = {
    type: 'bar',
    data: data,
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
var ctx = document.getElementById("myChart").getContext('2d');
ctx.width = 100;
ctx.height = 100;
var my = new Chart(ctx, 
    config
);