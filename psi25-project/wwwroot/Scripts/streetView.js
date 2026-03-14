const urlParams = new URLSearchParams(window.location.search);
const totalRounds = parseInt(urlParams.get('rounds')) || 1;

async function initMap() {
  let currentRound = parseInt(sessionStorage.getItem('currentRound')) || 1;
  let allResults = JSON.parse(sessionStorage.getItem('allResults')) || [];
  let gameId = sessionStorage.getItem('gameId');
  if (!gameId) {
    const responseGame = await postGame({ UserId: "f5f925f5-4345-474e-a068-5169ab8bcb15" }); // TODO get actual user id
    gameId = responseGame.id;
    sessionStorage.setItem('gameId', gameId);
  }

  const response = await fetch('/api/geocoding/valid_coords');
  const data = await response.json();

  // Convert to numbers and validate
  const lat = parseFloat(data.modifiedCoordinates.lat);
  const lng = parseFloat(data.modifiedCoordinates.lng);

  const initialPosition = { lat, lng };

  new google.maps.StreetViewPanorama(
    document.getElementById('street-view'),
    {
      position: initialPosition,
      pov: { heading: 0, pitch: 0 },
      zoom: 1,
    }
  );

  const responseLocation = await postLocation({ latitude: lat, longitude: lng, panoId: data.panoID });

  if (!responseLocation) {
    showMessage('Error initializing game. Please try again later.', 'error');
    return;
  }

  const miniMap = new google.maps.Map(document.getElementById('mini-map'), {
    center: { lat: 0.0, lng: 0.0},
    zoom: 1,
    streetViewControl: false,
    fullscreenControl: false,
    mapTypeControl: false,
  });

  const selectionMarker = new google.maps.Marker({
    map: miniMap,
    visible: false,
  });

  const selectedCoords = { lat: null, lng: null };

  miniMap.addListener('click', (event) => {
    const lat = event.latLng.lat();
    const lng = event.latLng.lng();
    const position = { lat, lng };

    selectedCoords.lat = lat;
    selectedCoords.lng = lng;

    selectionMarker.setPosition(position);
    selectionMarker.setVisible(true);

    console.log('Selected coordinates:', {
      lat: lat.toFixed(5),
      lng: lng.toFixed(5),
    });
  });

  const submitButton = document.getElementById('submit-button');

  submitButton.addEventListener('click', async () => {
    if (selectedCoords.lat === null || selectedCoords.lng === null) {
      showMessage('Please select a location on the mini-map before submitting.', 'error');
      return;
    }

    console.log('Submitting coordinates:', selectedCoords);

    const guessData = {
      initialCoords: initialPosition,
      guessedCoords: selectedCoords
    };

    sessionStorage.setItem('guessData', JSON.stringify(guessData));
    const result = await getResult(guessData);
    sessionStorage.setItem('result', JSON.stringify(result));
    const updatedGame = await updateScore(gameId, result.score);

    const guessPayload = {
      gameId: gameId,
      locationId: responseLocation.id,
      guessedLatitude: selectedCoords.lat,
      guessedLongitude: selectedCoords.lng,
      distanceKm: result.distance,
      score: result.score
    };

    const createdGuess = await postGuess(guessPayload);

    if (currentRound < totalRounds) {
      currentRound++;
      sessionStorage.setItem('currentRound', currentRound);
      allResults.push(result);
      sessionStorage.setItem('allResults', JSON.stringify(allResults));
      window.location.reload();
    } else {
      sessionStorage.removeItem('currentRound');
      allResults.push(result);
      sessionStorage.setItem('allResults', JSON.stringify(allResults));
      const finishedGame = await finishGame();
      const totalScore = await fetchTotalScore(gameId);
      sessionStorage.setItem('totalScore', totalScore);
      sessionStorage.removeItem('gameId');
      window.location.href = '/result.html';
    }
  })
}

function showMessage(text, type) {
    const messageDiv = document.getElementById('message-container');
    messageDiv.textContent = text;
    messageDiv.className = type;
    
    // Clear message after 5 seconds
    setTimeout(() => {
        messageDiv.textContent = '';
        messageDiv.className = '';
    }, 5000);
}

async function getResult(data) {
  try {
    const response = await fetch('/api/result', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });
    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Error fetching result:', error);
    return null;
  }
}

async function postLocation(data) {
  try {
    const response = await fetch("api/Locations", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });
    const location = await response.json();
    return location;
  } catch (error) {
    console.error('Error posting location:', error);
    return null;
  }
}

async function postGame(data) {
  try {
    const response = await fetch("api/Game", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
  });
    const game = await response.json();
    return game;
  } catch (error) {
    console.error('Error posting game:', error);
    return null;
  }
}

async function updateScore(gameId, score) {
  try {
    const response = await fetch(`api/Game/${gameId}/score`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(score)
    });
    const updatedGame = await response.json();
    return updatedGame;
  } catch (error) {
    console.error('Error updating score:', error);
    return null;
  }
}

async function postGuess(data) {
  try {
    const response = await fetch("api/Guess", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });
    const guess = await response.json();
    return guess;
  } catch (error) {
    console.error('Error posting guess:', error);
    return null;
  }
}

async function finishGame() {
  let gameId = sessionStorage.getItem('gameId');
  try {
    const response = await fetch(`api/Game/${gameId}/finish`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      }
    });
    const finishedGame = await response.json();
    return finishedGame;
  } catch (error) {
    console.error('Error finishing game:', error);
    return null;
  }
}

async function fetchTotalScore(gameId) {
    const response = await fetch(`/api/Game/${gameId}/total-score`);
    if (!response.ok) return 0;
    return await response.json();
}

