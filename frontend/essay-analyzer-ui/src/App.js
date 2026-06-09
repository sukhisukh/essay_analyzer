import { useState } from 'react';
import axios from 'axios';
import './App.css';
import EssayInput from './components/EssayInput';
import LoadingSpinner from './components/LoadingSpinner';
import ResultsDashboard from './components/ResultsDashboard';

// Your backend API URL — Codespaces forwarded port
const API_URL = 'https://essay-analyzer-api-d5d6fgfddqfgapgk.westus3-01.azurewebsites.net';

function App() {
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState(null);
  const [error, setError] = useState(null); 

  const handleSubmit = async (essayText) => {
    setLoading(true);
    setError(null);
    
    try {
      // Real API call to your ASP.NET Core backend
      const response = await axios.post(`${API_URL}/api/essays/analyze`, {
        essayText: essayText
      });

      setResults(response.data);

    } catch (err) {
      console.error('API Error:', err);
      setError('Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setResults(null);  // clears results, goes back to input
  };

  return (
    <div style={{ fontFamily: 'Arial, sans-serif', minHeight: '100vh', backgroundColor: '#f5f7fa' }}>
      
      {/* Header */}
      <div style={{ 
        backgroundColor: '#1B3A6B', 
        color: 'white', 
        padding: '20px', 
        textAlign: 'center',
        marginBottom: '20px'
      }}>
        <h1 style={{ margin: 0 }}>📝 Essay Analyzer</h1>
        <p style={{ margin: '5px 0 0 0', opacity: 0.8 }}>
          AI-powered writing feedback for high school students
        </p>
      </div>

      {/* Error message */}
      {error && (
        <div style={{
          maxWidth: '800px',
          margin: '0 auto 20px auto',
          padding: '15px',
          backgroundColor: '#fde8e8',
          border: '1px solid #e74c3c',
          borderRadius: '8px',
          color: '#e74c3c',
          textAlign: 'center'
        }}>
          ⚠️ {error}
        </div>
      )}

      {/* Main content */}
      {loading && <LoadingSpinner />}
      {!loading && !results && <EssayInput onSubmit={handleSubmit} />}
      {!loading && results && (
        <ResultsDashboard 
          results={results} 
          onReset={handleReset} 
        />
      )}

    </div>
  );
}

export default App;