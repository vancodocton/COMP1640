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
/*        scales: {
            y: {
                beginAtZero: true
            }
        }*/
        responsive: true,
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
const myChart = new Chart(
    document.getElementById('myChart'),
    config);