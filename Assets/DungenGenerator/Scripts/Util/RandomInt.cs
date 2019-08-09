using UnityEngine;
public class RandomInt
{
    int m_seed = 0;
    bool m_isInit = false;
    Random.State m_state;
       
    public void Init(int _seed = 0) {
        if (!m_isInit) {
            m_seed = _seed;
            m_isInit = true;

            Random.InitState(m_seed);
            
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_min">包括最小值</param>
    /// <param name="_max">不包括最大值</param>
    /// <returns></returns>
    public int GetValue(int _min,int _max) {
  
        return Random.Range(_min, _max);
    }
    public float GetValue() {
        return Random.Range(0f, 1f);
    }
}
