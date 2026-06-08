import { useState } from 'react';
import axios from 'axios';
import './App.css';
import EssayInput from './components/EssayInput';
import LoadingSpinner from './components/LoadingSpinner';
import ResultsDashboard from './components/ResultsDashboard';

// Your backend API URL — Codespaces forwarded port
const API_URL = 'git add .https://essay-analyzer-api-d5d6fgfddqfgapgk.westus3-01.azurewebsites.net';

// Fake results to test UI before real API is connected
const FAKE_RESULTS = {
  overallScore: 4,
  summary: "This essay presents a clear argument with solid evidence. The writer demonstrates good understanding of the topic but could strengthen transitions between paragraphs.",
  categories: [
    { name: "Thesis & Argument", score: 4, feedback: "Your thesis is clear and well-positioned. Consider making your main claim even more specific in the opening sentence." },
    { name: "Evidence & Support", score: 3, feedback: "Good use of examples, but try to explain how each piece of evidence connects back to your thesis." },
    { name: "Organization & Flow", score: 4, feedback: "Strong paragraph structure. Adding transition sentences between paragraphs would improve the overall flow." },
    { name: "Grammar & Style", score: 3, feedback: "A few run-on sentences detected. Try breaking long sentences into two for clarity." }
  ],
  topStrength: "Your opening paragraph immediately establishes a clear position and draws the reader in effectively.",
  topImprovement: "Focus on connecting your evidence more explicitly to your thesis — explain the 'so what' after each example."
};

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