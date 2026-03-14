import { Routes, Route } from 'react-router-dom'
import Home from './pages/Home'
import Start from './pages/Start'
import Game from './pages/Game'
import Results from './pages/Results'
import Signup from './pages/Signup'
import Login from './pages/Login'
import JoinRoom from './pages/JoinRoom';
import RoomLobby from './pages/RoomLobby'
import MultiplayerGame from './pages/MultiplayerGame'
import UserPage from './pages/UserPage'
import Layout from './components/Layout'
import About from './pages/About'

export default function App() {
  return (
    <Routes>    
      <Route path="/game" element={<Game />} />
      <Route path="/multiplayer/:roomCode/:gameId" element={<MultiplayerGame />} />
      <Route path="/results/:gameId" element={<Results />} />
      <Route path="/results" element={<Results/>}/>
      <Route path="/signup" element={<Signup/>}/>
      <Route path="/login" element={<Login/>}/>
      <Route path="/joinroom" element={<JoinRoom />} />
      <Route path="/roomlobby" element={<RoomLobby />} />
      <Route path="/room/:roomCode" element={<RoomLobby />} />
      <Route path="*" element={<div className="p-6">404 - Page Not Found</div>} />
      <Route element={<Layout/>}>
        <Route path="/user" element={<UserPage/>}/>
        <Route path="/start" element={<Start />} />
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About/>}/>
      </Route>
    </Routes>
  )
}