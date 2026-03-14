document.addEventListener('DOMContentLoaded', () => {
    const allResults = JSON.parse(sessionStorage.getItem('allResults')) || [];

    allResults.forEach((result, i) => {
        const div = document.createElement('div');
        div.innerHTML = `
            <h3>Round ${i + 1}</h3>
            <p>Initial coordinates: (${result.initialCoords.lat.toFixed(4)}, ${result.initialCoords.lng.toFixed(4)})</p>
            <p>Guessed coordinates: (${result.guessedCoords.lat.toFixed(4)}, ${result.guessedCoords.lng.toFixed(4)})</p>
            <p>Distance: ${result.distance} kilometers</p>
            <p>Score: ${result.score} points</p>
            <hr/>
        `;
        document.getElementById('round-result').appendChild(div);
    });

    const totalScore = parseInt(sessionStorage.getItem('totalScore')) || 0;
    const overallDiv = document.createElement('div');
    overallDiv.innerHTML = `<h2>Total Score: ${totalScore} points</h2>`;
    document.getElementById('overall-result').appendChild(overallDiv);

    sessionStorage.removeItem('allResults');
    sessionStorage.removeItem('totalScore');
});