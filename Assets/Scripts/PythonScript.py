import UnityEngine as ue
import numpy as np

gameManager = ue.GameObject.Find("GameManager")
ManagerComponent = gameManager.GetComponent("GameManager")
ue.Debug.Log(ManagerComponent.Timings[0,0])

data = np.array(range(24),dtype=object).reshape(12,2)
for x in range(12):
	data[x,0] = ManagerComponent.Timings[x,0]
	data[x,1] = ManagerComponent.Timings[x,1]
ue.Debug.Log(data[0,0])

np.save('Data.npy' , data)

